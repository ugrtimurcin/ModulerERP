import { create } from 'zustand';

// ============================================
// TOAST STORE
// ============================================
export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
    id: string;
    type: ToastType;
    title: string;
    message?: string;
    duration?: number;
}

interface ToastStore {
    toasts: Toast[];
    addToast: (toast: Omit<Toast, 'id'>) => void;
    removeToast: (id: string) => void;
    success: (title: string, message?: string) => void;
    error: (title: string, message?: string) => void;
    warning: (title: string, message?: string) => void;
    info: (title: string, message?: string) => void;
}

export const useToastStore = create<ToastStore>((set, get) => ({
    toasts: [],

    addToast: (toast) => {
        const id = Math.random().toString(36).substring(2, 9);
        const duration = toast.duration ?? 5000;

        set((state) => ({
            toasts: [...state.toasts, { ...toast, id }],
        }));

        if (duration > 0) {
            setTimeout(() => {
                get().removeToast(id);
            }, duration);
        }
    },

    removeToast: (id) => {
        set((state) => ({
            toasts: state.toasts.filter((t) => t.id !== id),
        }));
    },

    success: (title, message) => {
        get().addToast({ type: 'success', title, message });
    },

    error: (title, message) => {
        get().addToast({ type: 'error', title, message, duration: 8000 });
    },

    warning: (title, message) => {
        get().addToast({ type: 'warning', title, message });
    },

    info: (title, message) => {
        get().addToast({ type: 'info', title, message });
    },
}));

// ============================================
// DIALOG STORE
// ============================================
export interface DialogConfig {
    title: string;
    message: string;
    type?: 'confirm' | 'alert' | 'danger';
    confirmText?: string;
    cancelText?: string;
    onConfirm?: () => void | Promise<void>;
    onCancel?: () => void;
}

interface DialogStore {
    isOpen: boolean;
    config: DialogConfig | null;
    isLoading: boolean;
    openDialog: (config: DialogConfig) => void;
    closeDialog: () => void;
    setLoading: (loading: boolean) => void;
    confirm: (config: Omit<DialogConfig, 'type'>) => Promise<boolean>;
    alert: (title: string, message: string) => Promise<void>;
    danger: (config: Omit<DialogConfig, 'type'>) => Promise<boolean>;
}

let resolvePromise: ((value: boolean) => void) | null = null;

export const useDialogStore = create<DialogStore>((set) => ({
    isOpen: false,
    config: null,
    isLoading: false,

    openDialog: (config) => {
        set({ isOpen: true, config, isLoading: false });
    },

    closeDialog: () => {
        set({ isOpen: false, config: null, isLoading: false });
        if (resolvePromise) {
            resolvePromise(false);
            resolvePromise = null;
        }
    },

    setLoading: (loading) => {
        set({ isLoading: loading });
    },

    confirm: (config) => {
        return new Promise((resolve) => {
            resolvePromise = resolve;
            set({
                isOpen: true,
                isLoading: false,
                config: {
                    ...config,
                    type: 'confirm',
                    onConfirm: async () => {
                        if (config.onConfirm) {
                            set({ isLoading: true });
                            await config.onConfirm();
                        }
                        set({ isOpen: false, config: null, isLoading: false });
                        resolve(true);
                        resolvePromise = null;
                    },
                    onCancel: () => {
                        set({ isOpen: false, config: null });
                        resolve(false);
                        resolvePromise = null;
                    },
                },
            });
        });
    },

    alert: (title, message) => {
        return new Promise((resolve) => {
            set({
                isOpen: true,
                isLoading: false,
                config: {
                    title,
                    message,
                    type: 'alert',
                    confirmText: 'OK',
                    onConfirm: () => {
                        set({ isOpen: false, config: null });
                        resolve();
                    },
                },
            });
        });
    },

    danger: (config) => {
        return new Promise((resolve) => {
            resolvePromise = resolve;
            set({
                isOpen: true,
                isLoading: false,
                config: {
                    ...config,
                    type: 'danger',
                    onConfirm: async () => {
                        if (config.onConfirm) {
                            set({ isLoading: true });
                            await config.onConfirm();
                        }
                        set({ isOpen: false, config: null, isLoading: false });
                        resolve(true);
                        resolvePromise = null;
                    },
                    onCancel: () => {
                        set({ isOpen: false, config: null });
                        resolve(false);
                        resolvePromise = null;
                    },
                },
            });
        });
    },
}));
