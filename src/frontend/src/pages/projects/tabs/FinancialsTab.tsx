import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import type { ProjectFinancialSummaryDto, ProjectDto, ProjectBudgetLineDto, CreateBudgetLineDto, UpdateBudgetLineDto } from '@/types/project';
import { projectService } from '@/services/projectService';
import { ArrowUpRight, ArrowDownRight, DollarSign, Wallet, Plus, Pencil, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui';
import { DataTable, type Column } from '@/components/ui/DataTable';
import { useDialog } from '@/components/ui/Dialog';
import { BudgetLineDialog } from '../components/BudgetLineDialog';

interface FinancialsTabProps {
    projectId: string;
}

export function FinancialsTab({ projectId }: FinancialsTabProps) {
    const { t } = useTranslation();
    const { confirm } = useDialog();
    const [loading, setLoading] = useState(true);
    const [summary, setSummary] = useState<ProjectFinancialSummaryDto | null>(null);
    const [project, setProject] = useState<ProjectDto | null>(null);

    // Budget Dialog State
    const [isBudgetDialogOpen, setIsBudgetDialogOpen] = useState(false);
    const [selectedBudgetLine, setSelectedBudgetLine] = useState<ProjectBudgetLineDto | undefined>(undefined);

    useEffect(() => {
        loadData();
    }, [projectId]);

    const loadData = async () => {
        setLoading(true);
        try {
            const [summaryRes, projectRes] = await Promise.all([
                projectService.financials.getSummary(projectId),
                projectService.projects.getById(projectId)
            ]);

            if (summaryRes.success && summaryRes.data) {
                setSummary(summaryRes.data);
            }
            if (projectRes.success && projectRes.data) {
                setProject(projectRes.data);
            }
        } catch (error) {
            console.error('Failed to load financial data', error);
        } finally {
            setLoading(false);
        }
    };

    const handleAddBudgetLine = () => {
        setSelectedBudgetLine(undefined);
        setIsBudgetDialogOpen(true);
    };

    const handleEditBudgetLine = (line: ProjectBudgetLineDto) => {
        setSelectedBudgetLine(line);
        setIsBudgetDialogOpen(true);
    };

    const handleDeleteBudgetLine = async (line: ProjectBudgetLineDto) => {
        confirm({
            title: t('common.delete'),
            message: t('common.deleteConfirmation'),
            confirmText: t('common.delete'),
            onConfirm: async () => {
                try {
                    await projectService.projects.deleteBudgetLine(projectId, line.id);
                    loadData(); // Reload to refresh list and totals
                } catch (error) {
                    console.error('Failed to delete budget line', error);
                }
            }
        });
    };

    const handleBudgetSubmit = async (data: CreateBudgetLineDto | UpdateBudgetLineDto) => {
        try {
            if (selectedBudgetLine) {
                await projectService.projects.updateBudgetLine(projectId, selectedBudgetLine.id, data as UpdateBudgetLineDto);
            } else {
                await projectService.projects.addBudgetLine(projectId, data as CreateBudgetLineDto);
            }
            loadData();
            setIsBudgetDialogOpen(false);
        } catch (error) {
            console.error('Failed to save budget line', error);
            throw error; // Let dialog handle loading state reset if needed
        }
    };

    if (loading) return <div>{t('common.loading')}</div>;
    if (!summary || !project) return <div>{t('common.noData')}</div>;

    const formatCurrency = (amount: number, currency = 'TRY') => {
        return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: currency }).format(amount);
    };

    const isOverBudget = summary.totalCost > summary.contractAmount;

    // Budget Lines Columns
    const budgetColumns: Column<ProjectBudgetLineDto>[] = [
        { key: 'costCode', header: t('common.code'), width: '15%' },
        { key: 'description', header: t('common.description'), width: '30%' },
        {
            key: 'category',
            header: t('common.category'),
            width: '15%',
            render: (item) => {
                const categories = [
                    t('projects.financials.material'),
                    t('projects.financials.labor'),
                    t('projects.financials.subcontractor'),
                    t('projects.financials.expense'),
                    t('projects.financials.equipment'),
                    t('projects.financials.overhead')
                ];
                return categories[item.category] || item.category;
            }
        },
        {
            key: 'quantity',
            header: t('common.quantity'),
            align: 'right',
            width: '15%',
            render: (item) => item.quantity.toLocaleString()
        },
        {
            key: 'unitPrice',
            header: t('projects.financials.unitPrice'),
            align: 'right',
            width: '15%',
            render: (item) => formatCurrency(item.unitPrice)
        },
        {
            key: 'totalAmount',
            header: t('common.total'),
            align: 'right',
            width: '15%',
            render: (item) => formatCurrency(item.totalAmount)
        },
    ];

    const budgetActions = (item: ProjectBudgetLineDto) => (
        <div className="flex justify-end gap-2">
            <Button variant="ghost" size="icon" onClick={() => handleEditBudgetLine(item)}>
                <Pencil className="h-4 w-4" />
            </Button>
            <Button variant="ghost" size="icon" onClick={() => handleDeleteBudgetLine(item)} className="text-red-500 hover:text-red-700 hover:bg-red-50">
                <Trash2 className="h-4 w-4" />
            </Button>
        </div>
    );

    return (
        <div className="space-y-8">
            {/* Financial Summary Cards */}
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <div className="rounded-xl border bg-card text-card-foreground shadow p-6">
                    <div className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <h3 className="tracking-tight text-sm font-medium">{t('projects.financials.contractValue')}</h3>
                        <DollarSign className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div className="text-2xl font-bold">{formatCurrency(summary.contractAmount, summary.currencyCode)}</div>
                </div>
                <div className="rounded-xl border bg-card text-card-foreground shadow p-6">
                    <div className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <h3 className="tracking-tight text-sm font-medium">{t('projects.financials.cost')}</h3>
                        <ArrowDownRight className={`h-4 w-4 ${isOverBudget ? 'text-red-500' : 'text-muted-foreground'}`} />
                    </div>
                    <div className={`text-2xl font-bold ${isOverBudget ? 'text-red-500' : ''}`}>
                        {formatCurrency(summary.totalCost, summary.currencyCode)}
                    </div>
                </div>
                <div className="rounded-xl border bg-card text-card-foreground shadow p-6">
                    <div className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <h3 className="tracking-tight text-sm font-medium">{t('projects.financials.billed')}</h3>
                        <ArrowUpRight className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div className="text-2xl font-bold">{formatCurrency(summary.totalBilled, summary.currencyCode)}</div>
                </div>
                <div className="rounded-xl border bg-card text-card-foreground shadow p-6">
                    <div className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <h3 className="tracking-tight text-sm font-medium">{t('projects.financials.profit')}</h3>
                        <Wallet className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div className={`text-2xl font-bold ${summary.projectedProfit < 0 ? 'text-red-500' : 'text-green-500'}`}>
                        {formatCurrency(summary.projectedProfit, summary.currencyCode)}
                    </div>
                </div>
            </div>

            {/* Budget Lines Section */}
            <div className="space-y-4">
                <div className="flex justify-between items-center">
                    <div>
                        <h3 className="text-lg font-medium">{t('projects.financials.budgetBreakdown')}</h3>
                        <p className="text-sm text-muted-foreground">
                            {t('projects.financials.totalBudget')}: <span className="font-bold text-foreground">{formatCurrency(project.totalBudget)}</span>
                        </p>
                    </div>
                    <Button onClick={handleAddBudgetLine}>
                        <Plus className="mr-2 h-4 w-4" />
                        {t('projects.financials.addBudgetLine')}
                    </Button>
                </div>

                <DataTable
                    data={project.budgetLines || []}
                    columns={budgetColumns}
                    keyField="id"
                    actions={budgetActions}
                />
            </div>

            {/* Dialog */}
            <BudgetLineDialog
                isOpen={isBudgetDialogOpen}
                onClose={() => setIsBudgetDialogOpen(false)}
                onSubmit={handleBudgetSubmit}
                initialData={selectedBudgetLine}
                projectId={projectId}
            />
        </div>
    );
}
