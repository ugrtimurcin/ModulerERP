import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2 } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { api } from '@/lib/api';
import { EarningDeductionTypeDialog } from './EarningDeductionTypeDialog';

interface EarningDeductionType {
    id: string;
    name: string;
    type: number; // 0 = Earning, 1 = Deduction
    category: number; // 0=Basic, 1=Bonus, 2=Overtime, 3=Tax, 4=Penalty
    isTaxable: boolean;
    isSgkSubject: boolean;
    multiplier: number;
}

export function EarningDeductionTypesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const [types, setTypes] = useState<EarningDeductionType[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [selectedType, setSelectedType] = useState<EarningDeductionType | null>(null);

    const loadData = async () => {
        setIsLoading(true);
        try {
            const res = await api.get('/hr/earning-deduction-types');
            const data = (res as any).data || res;
            setTypes(Array.isArray(data) ? data : data.items || []);
        } catch (error) {
            toast.error(t('common.error'));
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => { loadData(); }, []);

    const handleDelete = async (id: string) => {
        if (!window.confirm(t('common.confirmDelete'))) return;
        try {
            await api.delete(`/hr/earning-deduction-types/${id}`);
            toast.success(t('common.deleted'));
            loadData();
        } catch (error) {
            toast.error(t('common.error'));
        }
    };

    const getTypeColor = (type: number) => type === 0 ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700';
    const getTypeText = (type: number) => type === 0 ? 'Earning' : 'Deduction';

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h2 className="text-xl font-semibold">{t('hr.earningDeductionTypes', 'Earning & Deduction Types')}</h2>
                    <p className="text-sm text-[hsl(var(--muted-foreground))]">Configure payroll parameters and taxes.</p>
                </div>
                <Button onClick={() => { setSelectedType(null); setIsDialogOpen(true); }}>
                    <Plus className="w-4 h-4 mr-2" /> {t('common.add')}
                </Button>
            </div>

            {isLoading ? (
                <div className="p-8 text-center text-muted-foreground">{t('common.loading')}...</div>
            ) : (
                <div className="bg-[hsl(var(--card))] rounded-lg border border-[hsl(var(--border))] overflow-hidden">
                    <table className="w-full text-sm text-left">
                        <thead className="bg-[hsl(var(--muted))] text-[hsl(var(--muted-foreground))]">
                            <tr>
                                <th className="px-4 py-3">{t('hr.name')}</th>
                                <th className="px-4 py-3">{t('hr.type', 'Type')}</th>
                                <th className="px-4 py-3">{t('hr.multiplier', 'Multiplier')}</th>
                                <th className="px-4 py-3">{t('hr.taxSubject', 'Subject to Tax')}</th>
                                <th className="px-4 py-3 text-right">{t('common.actions')}</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-[hsl(var(--border))]">
                            {types.map(item => (
                                <tr key={item.id} className="hover:bg-[hsl(var(--muted)/0.5)]">
                                    <td className="px-4 py-3 font-medium">{item.name}</td>
                                    <td className="px-4 py-3">
                                        <span className={`px-2 py-1 rounded-full text-xs font-semibold ${getTypeColor(item.type)}`}>
                                            {getTypeText(item.type)}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3">{item.multiplier.toFixed(2)}x</td>
                                    <td className="px-4 py-3">
                                        {item.isTaxable ? 'Yes' : 'No'} / {item.isSgkSubject ? 'SGK' : 'No SGK'}
                                    </td>
                                    <td className="px-4 py-3 text-right">
                                        <Button variant="ghost" className="h-8 w-8 p-0" onClick={() => { setSelectedType(item); setIsDialogOpen(true); }}>
                                            <Edit2 className="w-4 h-4" />
                                        </Button>
                                        <Button variant="ghost" className="h-8 w-8 p-0 text-red-500 hover:text-red-700 hover:bg-red-50" onClick={() => handleDelete(item.id)}>
                                            <Trash2 className="w-4 h-4" />
                                        </Button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            <EarningDeductionTypeDialog
                open={isDialogOpen}
                onClose={(saved: boolean) => { setIsDialogOpen(false); if (saved) loadData(); }}
                data={selectedType}
            />
        </div>
    );
}
