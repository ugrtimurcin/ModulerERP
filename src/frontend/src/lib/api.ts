
const BASE_URL = '/api';

interface RequestOptions extends RequestInit {
    params?: Record<string, string>;
}

async function request<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
    const { params, headers, ...rest } = options;

    // Build URL with query params
    const url = new URL(endpoint.startsWith('http') ? endpoint : `${window.location.origin}${BASE_URL}${endpoint}`);
    if (params) {
        Object.entries(params).forEach(([key, value]) => {
            if (value !== undefined && value !== null) {
                url.searchParams.append(key, value);
            }
        });
    }

    // Get token
    const token = localStorage.getItem('accessToken');

    // Merge headers
    const authHeaders = token ? { 'Authorization': `Bearer ${token}` } : {};
    const contentHeaders = options.body && typeof options.body === 'string'
        ? { 'Content-Type': 'application/json' }
        : {};

    const config: RequestInit = {
        ...rest,
        headers: {
            ...contentHeaders,
            ...authHeaders,
            ...headers,
        } as HeadersInit,
    };

    const response = await fetch(url.toString(), config);

    if (!response.ok) {
        // Handle 401 Unauthorized - potentially redirect to login or refresh token
        if (response.status === 401) {
            // Optional: Dispatch logging out event or refresh flow
            console.error('Unauthorized access. Token might be expired.');
        }

        const errorText = await response.text();
        throw new Error(errorText || `HTTP error! status: ${response.status}`);
    }

    // Handle 204 No Content
    if (response.status === 204) {
        return {} as T;
    }

    return response.json();
}

export const api = {
    get: <T>(endpoint: string, params?: Record<string, string>) => request<T>(endpoint, { method: 'GET', params }),
    post: <T>(endpoint: string, body: unknown) => request<T>(endpoint, { method: 'POST', body: JSON.stringify(body) }),
    put: <T>(endpoint: string, body: unknown) => request<T>(endpoint, { method: 'PUT', body: JSON.stringify(body) }),
    delete: <T>(endpoint: string) => request<T>(endpoint, { method: 'DELETE' }),
    patch: <T>(endpoint: string, body: unknown) => request<T>(endpoint, { method: 'PATCH', body: JSON.stringify(body) }),
};
