import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import type { ProjectFinancialSummaryDto } from '@/types/project';
import { projectService } from '@/services/projectService';
import { ArrowUpRight, ArrowDownRight, DollarSign, Wallet } from 'lucide-react';

interface FinancialsTabProps {
    projectId: string;
}

export function FinancialsTab({ projectId }: FinancialsTabProps) {
    const { t } = useTranslation();
    const [loading, setLoading] = useState(true);
    const [data, setData] = useState<ProjectFinancialSummaryDto | null>(null);

    useEffect(() => {
        loadFinancials();
    }, [projectId]);

    const loadFinancials = async () => {
        try {
            const response = await projectService.financials.getSummary(projectId);
            if (response.success && response.data) {
                setData(response.data);
            }
        } catch (error) {
            console.error('Failed to load financials', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div>{t('common.loading')}</div>;
    if (!data) return <div>{t('common.noData')}</div>;

    const formatCurrency = (amount: number, currency: string) => {
        return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: currency }).format(amount);
    };

    const isOverBudget = data.totalCost > data.contractAmount; // Simple check

    return (
        <div className="space-y-6">
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                {/* Contract Value */}
                <div className="rounded-xl border bg-card text-card-foreground shadow p-6">
                    <div className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <h3 className="tracking-tight text-sm font-medium">{t('projects.financials.contractValue')}</h3>
                        <DollarSign className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div className="text-2xl font-bold">{formatCurrency(data.contractAmount, data.currencyCode)}</div>
                </div>

                {/* Actual Cost */}
                <div className="rounded-xl border bg-card text-card-foreground shadow p-6">
                    <div className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <h3 className="tracking-tight text-sm font-medium">{t('projects.financials.cost')}</h3>
                        <ArrowDownRight className={`h-4 w-4 ${isOverBudget ? 'text-red-500' : 'text-muted-foreground'}`} />
                    </div>
                    <div className={`text-2xl font-bold ${isOverBudget ? 'text-red-500' : ''}`}>
                        {formatCurrency(data.totalCost, data.currencyCode)}
                    </div>
                </div>

                {/* Invoiced */}
                <div className="rounded-xl border bg-card text-card-foreground shadow p-6">
                    <div className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <h3 className="tracking-tight text-sm font-medium">{t('projects.financials.billed')}</h3>
                        <ArrowUpRight className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div className="text-2xl font-bold">{formatCurrency(data.totalBilled, data.currencyCode)}</div>
                </div>

                {/* Projected Profit */}
                <div className="rounded-xl border bg-card text-card-foreground shadow p-6">
                    <div className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <h3 className="tracking-tight text-sm font-medium">{t('projects.financials.profit')}</h3>
                        <Wallet className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div className={`text-2xl font-bold ${data.projectedProfit < 0 ? 'text-red-500' : 'text-green-500'}`}>
                        {formatCurrency(data.projectedProfit, data.currencyCode)}
                    </div>
                </div>
            </div>

            {/* Cost Breakdown Chart/List can go here */}
            <div className="rounded-xl border bg-card text-card-foreground shadow p-6">
                <h3 className="font-semibold mb-4">{t('projects.financials.breakdown')}</h3>
                <div className="space-y-2">
                    {data.costBreakdown.map((item, index) => (
                        <div key={index} className="flex justify-between border-b pb-2">
                            <span>{item.type}</span>
                            <span>{formatCurrency(item.amount, data.currencyCode)}</span>
                        </div>
                    ))}
                    {data.costBreakdown.length === 0 && <div className="text-muted-foreground">{t('common.noData')}</div>}
                </div>
            </div>
        </div>
    );
}
