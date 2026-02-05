import { useTranslation } from 'react-i18next';
import { AlertTriangle, Info, Loader2, X } from 'lucide-react';
import { useDialogStore } from '@/stores/ui';

export function Dialog() {
    const { t } = useTranslation();
    const { isOpen, config, isLoading, closeDialog } = useDialogStore();

    if (!isOpen || !config) return null;

    const isDanger = config.type === 'danger';
    const isAlert = config.type === 'alert';

    const handleConfirm = async () => {
        if (config.onConfirm) {
            await config.onConfirm();
        }
    };

    const handleCancel = () => {
        if (config.onCancel) {
            config.onCancel();
        } else {
            closeDialog();
        }
    };

    return (
        <div className="fixed inset-0 z-[90] flex items-center justify-center">
            {/* Backdrop */}
            <div
                className="absolute inset-0 bg-black/50 backdrop-blur-sm animate-fade-in"
                onClick={!isLoading ? handleCancel : undefined}
            />

            {/* Dialog */}
            <div className="relative bg-[hsl(var(--card))] rounded-2xl shadow-2xl max-w-md w-full mx-4 animate-scale-in">
                {/* Close button */}
                {!isLoading && !isAlert && (
                    <button
                        onClick={handleCancel}
                        className="absolute top-4 right-4 p-1 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                    >
                        <X className="w-5 h-5" />
                    </button>
                )}

                {/* Content */}
                <div className="p-6">
                    {/* Icon */}
                    <div
                        className={`
              w-12 h-12 rounded-full flex items-center justify-center mb-4 mx-auto
              ${isDanger
                                ? 'bg-red-100 dark:bg-red-900/30 text-red-600'
                                : 'bg-blue-100 dark:bg-blue-900/30 text-blue-600'}
            `}
                    >
                        {isDanger ? (
                            <AlertTriangle className="w-6 h-6" />
                        ) : (
                            <Info className="w-6 h-6" />
                        )}
                    </div>

                    {/* Title */}
                    <h3 className="text-lg font-semibold text-center mb-2">
                        {config.title}
                    </h3>

                    {/* Message */}
                    <p className="text-[hsl(var(--muted-foreground))] text-center text-sm">
                        {config.message}
                    </p>
                </div>

                {/* Actions */}
                <div className="flex gap-3 p-4 border-t border-[hsl(var(--border))]">
                    {!isAlert && (
                        <button
                            onClick={handleCancel}
                            disabled={isLoading}
                            className="flex-1 py-2.5 px-4 rounded-xl border border-[hsl(var(--border))] hover:bg-[hsl(var(--accent))] transition-colors disabled:opacity-50"
                        >
                            {config.cancelText || t('common.cancel')}
                        </button>
                    )}
                    <button
                        onClick={handleConfirm}
                        disabled={isLoading}
                        className={`
              flex-1 py-2.5 px-4 rounded-xl font-medium transition-colors
              flex items-center justify-center gap-2
              disabled:opacity-50
              ${isDanger
                                ? 'bg-red-600 hover:bg-red-700 text-white'
                                : 'bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white'}
            `}
                    >
                        {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
                        {config.confirmText || t('common.confirm')}
                    </button>
                </div>
            </div>
        </div>
    );
}

// Hook for easy access
import { useMemo } from 'react';

export function useDialog() {
    const confirm = useDialogStore(s => s.confirm);
    const alert = useDialogStore(s => s.alert);
    const danger = useDialogStore(s => s.danger);

    return useMemo(() => ({ confirm, alert, danger }), [confirm, alert, danger]);
}
