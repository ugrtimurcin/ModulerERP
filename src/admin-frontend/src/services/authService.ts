import api from './api';

export interface LoginRequest {
    email: string;
    password: string;
}

export interface LoginResponse {
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
    user: {
        id: string;
        email: string;
        firstName: string;
        lastName: string;
        roles: string[];
    };
}

export const authService = {
    login: async (data: LoginRequest): Promise<LoginResponse> => {
        const response = await api.post<{ success: boolean; data: LoginResponse }>('/auth/login', data);
        return response.data.data;
    },
    logout: () => {
        localStorage.removeItem('admin_token');
        localStorage.removeItem('admin_user');
    }
};
