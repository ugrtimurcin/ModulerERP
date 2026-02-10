import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash2, Award } from 'lucide-react';
import { DataTable, Button, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { CommissionRuleDialog } from './CommissionRuleDialog';

interface CommissionRule {
    id: string;
    role: string;
    minTargetAmount: number;
    percentage: number;
    basis: number; // Enum: 1=Invoiced, 2=Collected, 3=Profit
}

const API_BASE = '/api/hr/commission-rules';

export function CommissionRulesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [rules, setRules] = useState<CommissionRule[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await fetch(API_BASE, { cache: 'no-store' });
            if (res.ok) setRules(await res.json());
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleCreate = () => { setDialogOpen(true); };

    const handleDelete = async (rule: CommissionRule) => {
        const confirmed = await dialog.danger({
            title: t('hr.deleteCommissionRule'),
            message: t('hr.confirmDeleteCommissionRule'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            try {
                const res = await fetch(`${API_BASE}/${rule.id}`, { method: 'DELETE', cache: 'no-store' });
                if (res.ok) { toast.success(t('hr.commissionRuleDeleted')); loadData(); }
                else toast.error(t('common.error'));
            } catch { toast.error(t('common.error')); }
        }
    };

    const handleDialogClose = (saved: boolean) => {
        setDialogOpen(false);
        if (saved) loadData();
    };

    const getBasisLabel = (basis: number) => {
        switch (basis) {
            case 1: return t('hr.basisInvoiced');
            case 2: return t('hr.basisCollected');
            case 3: return t('hr.basisProfit');
            default: return basis;
        }
    };

    const columns: Column<CommissionRule>[] = [
        {
            key: 'role',
            header: t('hr.role'),
            render: (r) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-green-100 dark:bg-green-900/30 text-green-600 flex items-center justify-center">
                        <Award className="w-5 h-5" />
                    </div>
                    <span className="font-semibold">{r.role}</span>
                </div>
            ),
        },
        {
            key: 'minTargetAmount',
            header: t('hr.minTarget'),
            render: (r) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(r.minTargetAmount)
        },
        {
            key: 'percentage',
            header: t('hr.percentage'),
            render: (r) => <span>{r.percentage}%</span>
        },
        {
            key: 'basis',
            header: t('hr.basis'),
            render: (r) => <span className="px-2 py-1 rounded bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300 text-xs font-medium">{getBasisLabel(r.basis)}</span>
        }
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Award className="w-6 h-6" />
                        {t('hr.commissionRules')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.commissionRulesSubtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4" />
                    {t('hr.createCommissionRule')}
                </Button>
            </div>

            <DataTable
                data={rules}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                actions={(rule) => (
                    <button onClick={() => handleDelete(rule)} className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600" title={t('common.delete')}>
                        <Trash2 className="w-4 h-4" />
                    </button>
                )}
            />

            <CommissionRuleDialog open={dialogOpen} onClose={handleDialogClose} />
        </div>
    );
}
