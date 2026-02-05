import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { api } from '../../../services/api';
import { useToast, Modal, Select, Button, Input } from '@/components/ui';

interface ChequeActionDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    cheque: any;
    actionType: string; // 'endorse', 'bank', 'paid', 'bounce'
    onSuccess: () => void;
}

export const ChequeActionDialog = ({ open, onOpenChange, cheque, actionType, onSuccess }: ChequeActionDialogProps) => {
    const { t } = useTranslation();
    const toast = useToast();
    const [loading, setLoading] = useState(false);

    const [targets, setTargets] = useState<any[]>([]);

    const [formData, setFormData] = useState({
        targetId: '',
        description: ''
    });

    useEffect(() => {
        if (open) {
            loadTargets();
        }
    }, [open, actionType]);

    const loadTargets = async () => {
        setTargets([]);
        if (actionType === 'endorse') {
            const res = await api.partners.getAll(1, 100, undefined, true); // Get Suppliers
            if (res.success && res.data) {
                setTargets(res.data.data.map((p: any) => ({ id: p.id, name: p.name })));
            }
        } else if (actionType === 'bank' || actionType === 'paid') {
            const res = await api.finance.accounts.getAll();
            if (res.success && res.data) {
                setTargets(res.data.filter((a: any) => a.isBankAccount).map((a: any) => ({ id: a.id, name: a.name })));
            }
        }
    };

    const getTitle = () => {
        switch (actionType) {
            case 'endorse': return t('cheques.actions.endorse', 'Endorse to Supplier');
            case 'bank': return t('cheques.actions.toBank', 'Send to Bank');
            case 'paid': return t('cheques.actions.markPaid', 'Mark as Paid');
            case 'bounce': return t('cheques.actions.markBounced', 'Mark as Bounced');
            default: return t('cheques.actions.updateStatus', 'Update Status');
        }
    };

    const getTargetLabel = () => {
        switch (actionType) {
            case 'endorse': return t('cheques.actions.selectSupplier', 'Select Supplier');
            case 'bank': return t('cheques.actions.selectBank', 'Select Bank Account');
            case 'paid': return t('cheques.actions.selectAccount', 'Select Account');
            default: return '';
        }
    };

    const getNewStatus = () => {
        switch (actionType) {
            case 'endorse': return 1; // Endorsed
            case 'bank': return 2; // BankCollection
            case 'paid': return 4; // Paid
            case 'bounce': return 5; // Bounced
            default: return 0;
        }
    };

    const handleSubmit = async () => {
        setLoading(true);
        const payload = {
            chequeId: cheque.id,
            newStatus: getNewStatus(),
            newLocationId: formData.targetId || null,
            description: formData.description
        };

        const res = await api.finance.cheques.updateStatus(payload);

        if (res.success) {
            toast.success(t('common.success', 'Success'), t('common.updatedSuccess', 'Updated successfully'));
            onOpenChange(false);
            onSuccess();
        } else {
            toast.error(t('common.error', 'Error'), res.error);
        }
        setLoading(false);
    };

    const footer = (
        <>
            <Button variant="secondary" onClick={() => onOpenChange(false)}>{t('common.cancel', 'Cancel')}</Button>
            <Button variant={actionType === 'bounce' ? 'danger' : 'primary'} onClick={handleSubmit} isLoading={loading}>
                {t('common.confirm', 'Confirm')}
            </Button>
        </>
    );

    return (
        <Modal
            isOpen={open}
            onClose={() => onOpenChange(false)}
            title={`${getTitle()} - ${cheque.chequeNumber}`}
            footer={footer}
        >
            <div className="space-y-4">
                {/* Target Selection */}
                {['endorse', 'bank', 'paid'].includes(actionType) && (
                    <Select
                        label={getTargetLabel()}
                        value={formData.targetId}
                        onChange={(e) => setFormData(prev => ({ ...prev, targetId: e.target.value }))}
                        options={[
                            { value: '', label: t('common.select', 'Select...') },
                            ...targets.map(t => ({ value: t.id, label: t.name }))
                        ]}
                    />
                )}

                <Input
                    label={t('common.description', 'Notes / Description')}
                    value={formData.description}
                    onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
                    placeholder={t('common.optional', 'Optional')}
                />
            </div>
        </Modal>
    );
};
