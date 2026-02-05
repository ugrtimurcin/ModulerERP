import { X, CheckCircle, XCircle, AlertTriangle, Info } from 'lucide-react';
import { useToastStore, type Toast } from '@/stores/ui';

const toastStyles = {
    success: {
        bg: 'bg-green-50 dark:bg-green-900/20',
        border: 'border-green-200 dark:border-green-800',
        text: 'text-green-800 dark:text-green-200',
        icon: CheckCircle,
        iconColor: 'text-green-500',
    },
    error: {
        bg: 'bg-red-50 dark:bg-red-900/20',
        border: 'border-red-200 dark:border-red-800',
        text: 'text-red-800 dark:text-red-200',
        icon: XCircle,
        iconColor: 'text-red-500',
    },
    warning: {
        bg: 'bg-amber-50 dark:bg-amber-900/20',
        border: 'border-amber-200 dark:border-amber-800',
        text: 'text-amber-800 dark:text-amber-200',
        icon: AlertTriangle,
        iconColor: 'text-amber-500',
    },
    info: {
        bg: 'bg-blue-50 dark:bg-blue-900/20',
        border: 'border-blue-200 dark:border-blue-800',
        text: 'text-blue-800 dark:text-blue-200',
        icon: Info,
        iconColor: 'text-blue-500',
    },
};

function ToastItem({ toast }: { toast: Toast }) {
    const removeToast = useToastStore((s) => s.removeToast);
    const style = toastStyles[toast.type];
    const Icon = style.icon;

    return (
        <div
            className={`
        flex items-start gap-3 p-4 rounded-xl border shadow-lg
        animate-slide-in-right
        ${style.bg} ${style.border} ${style.text}
      `}
            role="alert"
        >
            <Icon className={`w-5 h-5 flex-shrink-0 mt-0.5 ${style.iconColor}`} />
            <div className="flex-1 min-w-0">
                <p className="font-medium">{toast.title}</p>
                {toast.message && (
                    <p className="text-sm opacity-80 mt-0.5">{toast.message}</p>
                )}
            </div>
            <button
                onClick={() => removeToast(toast.id)}
                className="flex-shrink-0 p-1 rounded-lg hover:bg-black/10 dark:hover:bg-white/10 transition-colors"
            >
                <X className="w-4 h-4" />
            </button>
        </div>
    );
}

export function ToastContainer() {
    const toasts = useToastStore((s) => s.toasts);

    if (toasts.length === 0) return null;

    return (
        <div className="fixed top-4 right-4 z-[100] flex flex-col gap-2 max-w-sm w-full pointer-events-none">
            {toasts.map((toast) => (
                <div key={toast.id} className="pointer-events-auto">
                    <ToastItem toast={toast} />
                </div>
            ))}
        </div>
    );
}

// Hook for easy access
// Hook for easy access
import { useMemo } from 'react';

export function useToast() {
    const success = useToastStore(s => s.success);
    const error = useToastStore(s => s.error);
    const warning = useToastStore(s => s.warning);
    const info = useToastStore(s => s.info);

    return useMemo(() => ({ success, error, warning, info }), [success, error, warning, info]);
}
