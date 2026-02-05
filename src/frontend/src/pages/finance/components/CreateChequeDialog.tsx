import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { api } from '../../../services/api';
import { useToast, Modal, Input, Select, Button } from '@/components/ui';

interface CreateChequeDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess: () => void;
}

export const CreateChequeDialog = ({ open, onOpenChange, onSuccess }: CreateChequeDialogProps) => {
    const { t } = useTranslation();
    const toast = useToast();
    const [loading, setLoading] = useState(false);
    const [currencies, setCurrencies] = useState<any[]>([]);

    const [formData, setFormData] = useState({
        chequeNumber: '',
        type: '1', // 1=Own, 2=Customer
        bankName: '',
        branchName: '',
        accountNumber: '',
        dueDate: '',
        amount: '',
        currencyId: '',
        drawer: ''
    });

    useEffect(() => {
        if (open) {
            loadCurrencies();
        }
    }, [open]);

    const loadCurrencies = async () => {
        const res = await api.getCurrencies();
        if (res.success && res.data) {
            setCurrencies(res.data.filter(c => c.isActive));
            if (res.data.length > 0 && !formData.currencyId) {
                setFormData(prev => ({ ...prev, currencyId: res.data![0].id }));
            }
        }
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };



    const submit = async () => {
        if (!formData.chequeNumber || !formData.amount || !formData.currencyId || !formData.bankName || !formData.dueDate) {
            toast.error(t('common.error'), t('common.fillRequiredFields', 'Please fill all required fields'));
            return;
        }

        setLoading(true);

        const payload = {
            ...formData,
            type: parseInt(formData.type),
            amount: parseFloat(formData.amount),
            dueDate: new Date(formData.dueDate).toISOString()
        };

        const res = await api.finance.cheques.create(payload);

        if (res.success) {
            toast.success('Success', 'Cheque created successfully');
            onOpenChange(false);
            onSuccess();
            setFormData({
                chequeNumber: '',
                type: '1',
                bankName: '',
                branchName: '',
                accountNumber: '',
                dueDate: '',
                amount: '',
                currencyId: currencies[0]?.id || '',
                drawer: ''
            });
        } else {
            toast.error('Error', res.error);
        }
        setLoading(false);
    };

    const footer = (
        <>
            <Button type="button" variant="secondary" onClick={() => onOpenChange(false)}>{t('common.cancel', 'Cancel')}</Button>
            <Button type="button" variant="primary" onClick={submit} isLoading={loading}>{t('common.save', 'Save')}</Button>
        </>
    );

    return (
        <Modal
            isOpen={open}
            onClose={() => onOpenChange(false)}
            title={t('cheques.new', 'New Cheque / Promissory Note')}
            size="2xl"
            footer={footer}
        >
            <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label={t('cheques.chequeNumber', 'Cheque Number')}
                        name="chequeNumber"
                        value={formData.chequeNumber}
                        onChange={handleChange}
                        required
                    />
                    <Select
                        label={t('cheques.typeLabel', 'Type')}
                        name="type"
                        value={formData.type}
                        onChange={handleChange}
                        options={[
                            { value: '1', label: t('cheques.types.own', 'Own Cheque') },
                            { value: '2', label: t('cheques.types.customer', 'Customer Cheque') }
                        ]}
                    />
                </div>

                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label={t('cheques.bankName', 'Bank Name')}
                        name="bankName"
                        value={formData.bankName}
                        onChange={handleChange}
                        required
                    />
                    <Input
                        label={t('cheques.branchName', 'Branch')}
                        name="branchName"
                        value={formData.branchName}
                        onChange={handleChange}
                    />
                </div>

                <Input
                    label={t('cheques.accountNumber', 'Account Number')}
                    name="accountNumber"
                    value={formData.accountNumber}
                    onChange={handleChange}
                />

                <div className="grid grid-cols-3 gap-4">
                    <Input
                        label={t('cheques.amount', 'Amount')}
                        name="amount"
                        type="number"
                        step="0.01"
                        value={formData.amount}
                        onChange={handleChange}
                        required
                    />
                    <Select
                        label={t('common.currency', 'Currency')}
                        name="currencyId"
                        value={formData.currencyId}
                        onChange={handleChange}
                        required
                        options={currencies.map(c => ({ value: c.id, label: c.code }))}
                    />
                    <Input
                        label={t('cheques.dueDate', 'Due Date')}
                        name="dueDate"
                        type="date"
                        value={formData.dueDate}
                        onChange={handleChange}
                        required
                    />
                </div>

                <Input
                    label={t('cheques.drawer', 'Drawer / Owner Name')}
                    name="drawer"
                    value={formData.drawer}
                    onChange={handleChange}
                    placeholder="Person who signed the cheque"
                />
            </div>
        </Modal>
    );
};
