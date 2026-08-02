import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

type HttpMethod = 'GET' | 'POST';

interface DemoClient {
  name: string;
  role: string;
  apiKey: string;
  partnerId: string | null;
  purpose: string;
}

interface DemoEndpoint {
  id: string;
  label: string;
  method: HttpMethod;
  path: string;
  middlewareFocus: string;
  defaultClientIndex: number;
  includeApiKey: boolean;
  apiKeyOverride?: string;
  body?: () => unknown;
}

interface ApiResult {
  status: number | null;
  statusText: string;
  correlationId: string;
  body: unknown;
  error: boolean;
}

const demoClients: DemoClient[] = [
  {
    name: 'Retail Demo Client',
    role: 'RetailPartner',
    apiKey: 'retail-demo-key',
    partnerId: 'PARTNER-RETAIL-001',
    purpose: 'Reads products, inventory, and its own retail orders.'
  },
  {
    name: 'Distributor Demo Client',
    role: 'DistributorPartner',
    apiKey: 'distributor-demo-key',
    partnerId: 'PARTNER-DIST-001',
    purpose: 'Reads products, inventory, and its own distributor orders.'
  },
  {
    name: 'Analytics Demo Client',
    role: 'AnalyticsPartner',
    apiKey: 'analytics-demo-key',
    partnerId: null,
    purpose: 'Reads aggregated sales summaries only.'
  },
  {
    name: 'Compliance Demo Client',
    role: 'ComplianceUser',
    apiKey: 'compliance-demo-key',
    partnerId: null,
    purpose: 'Reads audit events only.'
  },
  {
    name: 'Internal Admin Demo Client',
    role: 'InternalAdmin',
    apiKey: 'admin-demo-key',
    partnerId: null,
    purpose: 'Can access partner, analytics, and compliance APIs.'
  }
];

@Component({
  selector: 'app-root',
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly clients = demoClients;
  protected readonly selectedClientIndex = signal(0);
  protected readonly selectedEndpointId = signal('products');
  protected readonly customCorrelationId = signal(this.createCorrelationId());
  protected readonly partnerId = signal('PARTNER-RETAIL-001');
  protected readonly productId = signal('PROD-001');
  protected readonly quantity = signal(5);
  protected readonly busy = signal(false);
  protected readonly result = signal<ApiResult | null>(null);

  protected readonly endpoints: DemoEndpoint[] = [
    {
      id: 'health',
      label: 'Health check',
      method: 'GET',
      path: '/health',
      middlewareFocus: 'Public route bypasses API-key validation and still returns a correlation id.',
      defaultClientIndex: 0,
      includeApiKey: false
    },
    {
      id: 'products',
      label: 'Products',
      method: 'GET',
      path: '/api/products',
      middlewareFocus: 'Authentication succeeds, access middleware allows partner roles, audit records the request.',
      defaultClientIndex: 0,
      includeApiKey: true
    },
    {
      id: 'inventory',
      label: 'Inventory',
      method: 'GET',
      path: '/api/inventory',
      middlewareFocus: 'Same role rule as products, useful for repeating a successful audited request.',
      defaultClientIndex: 1,
      includeApiKey: true
    },
    {
      id: 'own-orders',
      label: 'Own orders',
      method: 'GET',
      path: '/api/partners/PARTNER-RETAIL-001/orders',
      middlewareFocus: 'PartnerAccessMiddleware confirms the route partner id matches the API-key owner.',
      defaultClientIndex: 0,
      includeApiKey: true
    },
    {
      id: 'blocked-orders',
      label: 'Blocked partner orders',
      method: 'GET',
      path: '/api/partners/PARTNER-DIST-001/orders',
      middlewareFocus: 'A retail partner tries another tenant and receives 403 before the controller runs.',
      defaultClientIndex: 0,
      includeApiKey: true
    },
    {
      id: 'create-order',
      label: 'Create order',
      method: 'POST',
      path: '/api/partners/PARTNER-DIST-001/orders',
      middlewareFocus: 'Distributor access is allowed, then the controller validates and creates an order.',
      defaultClientIndex: 1,
      includeApiKey: true,
      body: () => ({ productId: this.productId(), quantity: this.quantity() })
    },
    {
      id: 'analytics',
      label: 'Sales summary',
      method: 'GET',
      path: '/api/analytics/sales-summary',
      middlewareFocus: 'Analytics role can read aggregate data but cannot access partner order routes.',
      defaultClientIndex: 2,
      includeApiKey: true
    },
    {
      id: 'audit',
      label: 'Audit events',
      method: 'GET',
      path: '/api/compliance/audit-events',
      middlewareFocus: 'Compliance role reads audit history; audit-log reads are intentionally not audited.',
      defaultClientIndex: 3,
      includeApiKey: true
    },
    {
      id: 'missing-key',
      label: 'Missing API key',
      method: 'GET',
      path: '/api/products',
      middlewareFocus: 'ApiKeyAuthenticationMiddleware returns 401 before audit or access rules have a client.',
      defaultClientIndex: 0,
      includeApiKey: false
    },
    {
      id: 'invalid-key',
      label: 'Invalid API key',
      method: 'GET',
      path: '/api/products',
      middlewareFocus: 'ApiKeyAuthenticationMiddleware rejects an unknown key with 401 before access rules run.',
      defaultClientIndex: 0,
      includeApiKey: true,
      apiKeyOverride: 'bad-demo-key'
    }
  ];

  protected readonly selectedClient = computed(() => this.clients[this.selectedClientIndex()]);
  protected readonly selectedEndpoint = computed(
    () => this.endpoints.find(endpoint => endpoint.id === this.selectedEndpointId()) ?? this.endpoints[0]
  );
  protected readonly requestBody = computed(() => this.selectedEndpoint().body?.() ?? null);
  protected readonly requestHeaders = computed(() => {
    const endpoint = this.selectedEndpoint();
    const headers: Record<string, string> = {
      'X-Correlation-Id': this.customCorrelationId()
    };

    if (endpoint.includeApiKey) {
      headers['X-Api-Key'] = endpoint.apiKeyOverride ?? this.selectedClient().apiKey;
    }

    return headers;
  });

  constructor(private readonly http: HttpClient) {}

  protected chooseEndpoint(endpoint: DemoEndpoint): void {
    this.selectedEndpointId.set(endpoint.id);
    this.selectedClientIndex.set(endpoint.defaultClientIndex);
    this.customCorrelationId.set(this.createCorrelationId());

    if (endpoint.id === 'create-order') {
      this.partnerId.set('PARTNER-DIST-001');
    } else if (endpoint.path.includes('PARTNER-DIST-001')) {
      this.partnerId.set('PARTNER-DIST-001');
    } else {
      this.partnerId.set('PARTNER-RETAIL-001');
    }
  }

  protected chooseClient(index: number): void {
    this.selectedClientIndex.set(index);
  }

  protected runSelectedRequest(): void {
    const endpoint = this.selectedEndpoint();
    const body = endpoint.body?.();
    const headers = new HttpHeaders(this.requestHeaders());

    this.busy.set(true);
    this.result.set(null);

    this.http.request(endpoint.method, endpoint.path, {
      body,
      headers,
      observe: 'response'
    }).subscribe({
      next: response => this.captureResponse(response, false),
      error: error => this.captureError(error)
    });
  }

  protected refreshCorrelationId(): void {
    this.customCorrelationId.set(this.createCorrelationId());
  }

  protected formatJson(value: unknown): string {
    return JSON.stringify(value, null, 2);
  }

  private captureResponse(response: HttpResponse<unknown>, error: boolean): void {
    this.result.set({
      status: response.status,
      statusText: response.statusText || 'OK',
      correlationId: response.headers.get('X-Correlation-Id') ?? '',
      body: response.body,
      error
    });
    this.busy.set(false);
  }

  private captureError(error: HttpErrorResponse): void {
    this.result.set({
      status: error.status || null,
      statusText: error.statusText || 'Request failed',
      correlationId: error.headers?.get('X-Correlation-Id') ?? '',
      body: error.error || { message: error.message },
      error: true
    });
    this.busy.set(false);
  }

  private createCorrelationId(): string {
    return `ui-${crypto.randomUUID().replaceAll('-', '').slice(0, 16)}`;
  }
}
