import { create } from 'zustand';
import { persist } from 'zustand/middleware';

// Theme Store
type Theme = 'light' | 'dark' | 'system';

interface ThemeState {
    theme: Theme;
    setTheme: (theme: Theme) => void;
}

export const useThemeStore = create<ThemeState>()(
    persist(
        (set) => ({
            theme: 'system',
            setTheme: (theme) => {
                set({ theme });
                applyTheme(theme);
            },
        }),
        { name: 'theme-storage' }
    )
);

function applyTheme(theme: Theme) {
    const root = document.documentElement;
    const systemDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

    if (theme === 'dark' || (theme === 'system' && systemDark)) {
        root.classList.add('dark');
    } else {
        root.classList.remove('dark');
    }
}

// Initialize theme on load
const savedTheme = localStorage.getItem('theme-storage');
if (savedTheme) {
    try {
        const { state } = JSON.parse(savedTheme);
        applyTheme(state.theme);
    } catch {
        applyTheme('system');
    }
}

// Sidebar Store
interface SidebarState {
    isCollapsed: boolean;
    toggle: () => void;
    setCollapsed: (collapsed: boolean) => void;
}

export const useSidebarStore = create<SidebarState>()(
    persist(
        (set) => ({
            isCollapsed: false,
            toggle: () => set((state) => ({ isCollapsed: !state.isCollapsed })),
            setCollapsed: (isCollapsed) => set({ isCollapsed }),
        }),
        { name: 'sidebar-storage' }
    )
);

// Auth Store
import { jwtDecode } from 'jwt-decode';

interface User {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    tenantId: string;
    roles: string[];
    permissions: string[];
}

interface AuthState {
    user: User | null;
    isAuthenticated: boolean;
    login: (user: Omit<User, 'permissions'>, accessToken: string, refreshToken: string) => void;
    logout: () => void;
    setUser: (user: User) => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            user: null,
            isAuthenticated: false,
            login: (userData, accessToken, refreshToken) => {
                localStorage.setItem('accessToken', accessToken);
                localStorage.setItem('refreshToken', refreshToken);

                let permissions: string[] = [];
                try {
                    const decoded = jwtDecode<{ permissions: string }>(accessToken);
                    if (decoded.permissions) {
                        permissions = JSON.parse(decoded.permissions);
                    }
                } catch (e) {
                    console.error("Failed to parse permissions from token", e);
                }

                const user: User = { ...userData, permissions };
                set({ user, isAuthenticated: true });
            },
            logout: () => {
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                set({ user: null, isAuthenticated: false });
            },
            setUser: (user) => set({ user }),
        }),
        { name: 'auth-storage' }
    )
);
