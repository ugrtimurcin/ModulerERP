import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Modal } from '@/components/ui/Modal';
import { Button, Input } from '@/components/ui';
import { api } from '@/lib/api';
import type { CreateChangeOrderDto } from '@/types/project';

interface ChangeOrderDialogProps {
    isOpen: boolean;
    onClose: () => void;
    projectId: string;
    onSuccess: () => void;
}

export function ChangeOrderDialog({ isOpen, onClose, projectId, onSuccess }: ChangeOrderDialogProps) {
    const { t } = useTranslation();
    const [loading, setLoading] = useState(false);
    const [form, setForm] = useState<CreateChangeOrderDto>({
        projectId,
        title: '',
        description: '',
        amountChange: 0,
        timeExtensionDays: 0
    });

    useEffect(() => {
        if (isOpen) {
            setForm({
                projectId,
                title: '',
                description: '',
                amountChange: 0,
                timeExtensionDays: 0
            });
        }
    }, [isOpen, projectId]);

    const handleSubmit = async () => {
        try {
            setLoading(true);
            await api.post('/projectchangeorders', form);
            onSuccess();
            onClose();
        } catch (error) {
            console.error('Failed to create change order', error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={t('projects.changeOrders.createTitle')}
        >
            <div className="space-y-4 py-4">
                <Input
                    label={t('projects.changeOrders.formTitle')}
                    value={form.title}
                    onChange={(e) => setForm({ ...form, title: e.target.value })}
                    placeholder="e.g. Additional Flooring"
                />

                <div className="space-y-1">
                    <label className="text-sm font-medium">{t('projects.changeOrders.description')}</label>
                    <textarea
                        className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        value={form.description}
                        onChange={(e) => setForm({ ...form, description: e.target.value })}
                        placeholder="Detailed explanation of the change..."
                    />
                </div>

                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label={t('projects.changeOrders.amountChange')}
                        type="number"
                        value={form.amountChange}
                        onChange={(e) => setForm({ ...form, amountChange: parseFloat(e.target.value) })}
                        placeholder="0.00"
                    />
                    <Input
                        label={t('projects.changeOrders.timeExtension')}
                        type="number"
                        value={form.timeExtensionDays}
                        onChange={(e) => setForm({ ...form, timeExtensionDays: parseInt(e.target.value) })}
                        placeholder="0"
                    />
                </div>

                <div className="flex justify-end space-x-2 mt-4">
                    <Button variant="ghost" onClick={onClose} disabled={loading}>
                        {t('common.cancel')}
                    </Button>
                    <Button onClick={handleSubmit} disabled={loading}>
                        {t('common.create')}
                    </Button>
                </div>
            </div>
        </Modal>
    );
}
