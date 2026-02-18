import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Shield, Plus, Pencil, Trash2 } from 'lucide-react';
import { DataTable, Button, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { SocialSecurityRuleDialog } from './SocialSecurityRuleDialog';
import { socialSecurityRuleService } from '@/services/hr/socialSecurityRuleService';
import type { SocialSecurityRule } from '@/services/hr/socialSecurityRuleService';
import { CitizenshipType, SocialSecurityType } from '@/types/hr';

export function SocialSecurityRulesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [rules, setRules] = useState<SocialSecurityRule[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [selectedRule, setSelectedRule] = useState<SocialSecurityRule | null>(null);
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const data = await socialSecurityRuleService.getAll();
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

    const handleEdit = (rule: SocialSecurityRule) => {
        setSelectedRule(rule);
        setIsDialogOpen(true);
    };

    const handleDelete = async (rule: SocialSecurityRule) => {
        const ok = await dialog.danger({
            title: t('common.deleteConfirmTitle'),
            message: t('common.deleteConfirmMessage'),
            confirmText: t('common.delete'),
            cancelText: t('common.cancel')
        });

        if (!ok) return;

        try {
            await socialSecurityRuleService.delete(rule.id);
            toast.success(t('hr.ssRuleDeleted'));
            loadData();
        } catch (error: any) {
            toast.error(error.message || t('common.error'));
        }
    };

    const getCitizenshipLabel = (val: number) => {
        const key = Object.keys(CitizenshipType).find(key => CitizenshipType[key as keyof typeof CitizenshipType] === val);
        return key ? t(`hr.citizenshipTypes.${key}`) : val;
    };

    const getSSLabel = (val: number) => {
        const key = Object.keys(SocialSecurityType).find(key => SocialSecurityType[key as keyof typeof SocialSecurityType] === val);
        return key ? t(`hr.socialSecurityTypes.${key}`) : val;
    };

    const columns: Column<SocialSecurityRule>[] = [
        {
            key: 'name',
            header: t('common.name'),
            render: (rule) => <span className="font-medium">{rule.name}</span>
        },
        {
            key: 'citizenshipType',
            header: t('hr.citizenship'),
            render: (rule) => <span className="text-sm px-2 py-1 rounded bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300">{getCitizenshipLabel(rule.citizenshipType)}</span>
        },
        {
            key: 'socialSecurityType',
            header: t('hr.socialSecurityType'),
            render: (rule) => <span className="text-sm px-2 py-1 rounded bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300">{getSSLabel(rule.socialSecurityType)}</span>
        },
        {
            key: 'employeeRate',
            header: t('hr.employeeRate'),
            render: (rule) => <span className="font-mono">{rule.employeeRate}%</span>
        },
        {
            key: 'employerRate',
            header: t('hr.employerRate'),
            render: (rule) => <span className="font-mono">{rule.employerRate}%</span>
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
                        <Shield className="w-6 h-6" />
                        {t('hr.socialSecurityRules')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.socialSecurityRulesSubtitle')}</p>
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

            <SocialSecurityRuleDialog
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
