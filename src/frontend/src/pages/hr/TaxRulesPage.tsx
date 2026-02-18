import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Percent, Plus, Pencil, Trash2 } from 'lucide-react';
import { DataTable, Button, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { TaxRuleDialog } from './TaxRuleDialog';
import { taxRuleService } from '@/services/hr/taxRuleService';
import type { TaxRule } from '@/services/hr/taxRuleService';

export function TaxRulesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [rules, setRules] = useState<TaxRule[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [selectedRule, setSelectedRule] = useState<TaxRule | null>(null);
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const data = await taxRuleService.getAll();
            setRules(data);
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleCreate = () => {
        setSelectedRule(null);
        setIsDialogOpen(true);
    };

    const handleEdit = (rule: TaxRule) => {
        setSelectedRule(rule);
        setIsDialogOpen(true);
    };

    const handleDelete = async (rule: TaxRule) => {
        const ok = await dialog.danger({
            title: t('common.deleteConfirmTitle'),
            message: t('common.deleteConfirmMessage'),
            confirmText: t('common.delete'),
            cancelText: t('common.cancel')
        });

        if (!ok) return;

        try {
            await taxRuleService.delete(rule.id);
            toast.success(t('hr.taxRuleDeleted'));
            loadData();
        } catch (error: any) {
            toast.error(error.message || t('common.error'));
        }
    };

    const formatCurrency = (amount: number | null) => {
        if (amount === null) return t('common.unlimited');
        return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(amount);
    };

    const columns: Column<TaxRule>[] = [
        {
            key: 'order',
            header: t('common.order'),
            render: (rule) => <span className="font-mono text-[hsl(var(--muted-foreground))]">#{rule.order}</span>
        },
        {
            key: 'name',
            header: t('common.name'),
            render: (rule) => <span className="font-medium">{rule.name}</span>
        },
        {
            key: 'lowerLimit',
            header: t('hr.lowerLimit'),
            render: (rule) => formatCurrency(rule.lowerLimit)
        },
        {
            key: 'upperLimit',
            header: t('hr.upperLimit'),
            render: (rule) => formatCurrency(rule.upperLimit)
        },
        {
            key: 'rate',
            header: t('hr.rate'),
            render: (rule) => <span className="font-bold text-blue-600">{rule.rate}%</span>
        },
        {
            key: 'effectiveFrom',
            header: t('common.effectiveFrom'),
            render: (rule) => new Date(rule.effectiveFrom).toLocaleDateString()
        }
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Percent className="w-6 h-6" />
                        {t('hr.taxRules')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.taxRulesSubtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4 mr-2" />
                    {t('common.create')}
                </Button>
            </div>

            <DataTable
                data={rules}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(rule) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => handleEdit(rule)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(rule)}
                            className="p-2 rounded-lg hover:bg-red-100 text-red-600 dark:hover:bg-red-900/30"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <TaxRuleDialog
                open={isDialogOpen}
                onClose={(saved) => {
                    setIsDialogOpen(false);
                    if (saved) loadData();
                }}
                rule={selectedRule}
            />
        </div>
    );
}
