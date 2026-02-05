import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, useToast } from '@/components/ui';
import { X } from 'lucide-react';

interface PublicHolidayDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
}

export function PublicHolidayDialog({ open, onClose }: PublicHolidayDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isLoading, setIsLoading] = useState(false);

    const [formData, setFormData] = useState({
        name: '',
        date: '',
        isHalfDay: false
    });

    useEffect(() => {
        if (open) {
            setFormData({ name: '', date: '', isHalfDay: false });
        }
    }, [open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);

        try {
            const res = await fetch('/api/hr/public-holidays', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData),
            });

            if (res.ok) {
                toast.success(t('hr.publicHolidayCreated'));
                onClose(true);
            } else {
                toast.error(t('common.error'));
            }
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    };

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-md border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{t('hr.createPublicHoliday')}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    <div>
                        <label className="block text-sm font-medium mb-1.5">{t('common.name')}</label>
                        <input
                            type="text"
                            required
                            value={formData.name}
                            onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1.5">{t('common.date')}</label>
                        <input
                            type="date"
                            required
                            value={formData.date}
                            onChange={(e) => setFormData(prev => ({ ...prev, date: e.target.value }))}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    <div className="flex items-center gap-2">
                        <input
                            type="checkbox"
                            id="isHalfDay"
                            checked={formData.isHalfDay}
                            onChange={(e) => setFormData(prev => ({ ...prev, isHalfDay: e.target.checked }))}
                            className="w-4 h-4 rounded border-[hsl(var(--border))] text-indigo-600 focus:ring-indigo-500"
                        />
                        <label htmlFor="isHalfDay" className="text-sm font-medium">{t('hr.isHalfDay')}</label>
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button type="submit" disabled={isLoading}>
                            {t('common.create')}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
