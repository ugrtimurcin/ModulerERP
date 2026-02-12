import { Input } from '@/components/ui';
import { useTranslation } from 'react-i18next';
import type { CreateProgressPaymentDto } from '@/types/project';

interface Step1Props {
    data: Partial<CreateProgressPaymentDto>;
    onChange: (data: Partial<CreateProgressPaymentDto>) => void;
}

export function Step1BasicInfo({ data, onChange }: Step1Props) {
    const { t } = useTranslation();

    const handleChange = (field: keyof CreateProgressPaymentDto, value: any) => {
        onChange({ ...data, [field]: value });
    };

    return (
        <div className="space-y-4 max-w-lg mx-auto py-4">
            <h3 className="text-lg font-medium text-center mb-6">{t('projects.payments.wizard.enterBasicInfo')}</h3>

            <Input
                label={t('common.date')}
                type="date"
                value={data.date}
                onChange={(e) => handleChange('date', e.target.value)}
                required
            />

            <div className="grid grid-cols-2 gap-4">
                <Input
                    label={t('common.periodStart')}
                    type="date"
                    value={data.periodStart}
                    onChange={(e) => handleChange('periodStart', e.target.value)}
                    required
                />
                <Input
                    label={t('common.periodEnd')}
                    type="date"
                    value={data.periodEnd}
                    onChange={(e) => handleChange('periodEnd', e.target.value)}
                    required
                />
            </div>

            <div className="flex items-center space-x-2 mt-4">
                <input
                    type="checkbox"
                    id="isExpense"
                    checked={data.isExpense}
                    onChange={(e) => handleChange('isExpense', e.target.checked)}
                    className="w-4 h-4 rounded border-gray-300"
                />
                <label htmlFor="isExpense" className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
                    {t('projects.payments.isExpensePayment')}
                </label>
            </div>
            <p className="text-sm text-muted-foreground mt-2">
                {t('projects.payments.wizard.draftNote')}
            </p>
        </div>
    );
}
