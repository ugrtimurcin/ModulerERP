import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2 } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { api } from '@/lib/api';
import { LeavePolicyDialog } from './LeavePolicyDialog';

interface LeavePolicy {
    id: string;
    name: string;
    description?: string;
    defaultDays: number;
    maxDaysCarryForward: number;
    requiresApproval: boolean;
    genderRestriction: number;
}

export function LeavePoliciesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const [policies, setPolicies] = useState<LeavePolicy[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [selectedPolicy, setSelectedPolicy] = useState<LeavePolicy | null>(null);

    const loadData = async () => {
        setIsLoading(true);
        try {
            const res = await api.get('/hr/leave-policies');
            const data = (res as any).data || res;
            setPolicies(Array.isArray(data) ? data : data.items || []);
        } catch (error) {
            console.error(error);
            toast.error(t('common.error'));
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    const handleDelete = async (id: string) => {
        if (!window.confirm(t('common.confirmDelete'))) return;
        try {
            await api.delete(`/hr/leave-policies/${id}`);
            toast.success(t('common.deleted'));
            loadData();
        } catch (error) {
            console.error(error);
            toast.error(t('common.error'));
        }
    };

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h2 className="text-xl font-semibold">{t('hr.leavePolicies', 'Leave Policies')}</h2>
                    <p className="text-sm text-[hsl(var(--muted-foreground))]">Manage dynamic leave allocations and restrictions.</p>
                </div>
                <Button onClick={() => { setSelectedPolicy(null); setIsDialogOpen(true); }}>
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
                                <th className="px-4 py-3 font-medium">{t('hr.name')}</th>
                                <th className="px-4 py-3 font-medium">{t('hr.defaultDays', 'Default Days')}</th>
                                <th className="px-4 py-3 font-medium">{t('hr.requiresApproval', 'Requires Approval')}</th>
                                <th className="px-4 py-3 font-medium text-right">{t('common.actions')}</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-[hsl(var(--border))]">
                            {policies.map(policy => (
                                <tr key={policy.id} className="hover:bg-[hsl(var(--muted)/0.5)]">
                                    <td className="px-4 py-3 font-medium">{policy.name}</td>
                                    <td className="px-4 py-3">{policy.defaultDays}</td>
                                    <td className="px-4 py-3">
                                        <span className={`px-2 py-1 rounded-full text-xs ${policy.requiresApproval ? 'bg-amber-100 text-amber-700' : 'bg-green-100 text-green-700'}`}>
                                            {policy.requiresApproval ? 'Yes' : 'No'}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3 text-right">
                                        <Button variant="ghost" className="h-8 w-8 p-0" onClick={() => { setSelectedPolicy(policy); setIsDialogOpen(true); }}>
                                            <Edit2 className="w-4 h-4" />
                                        </Button>
                                        <Button variant="ghost" className="h-8 w-8 p-0 text-red-500 hover:text-red-700 hover:bg-red-50" onClick={() => handleDelete(policy.id)}>
                                            <Trash2 className="w-4 h-4" />
                                        </Button>
                                    </td>
                                </tr>
                            ))}
                            {policies.length === 0 && (
                                <tr>
                                    <td colSpan={4} className="px-4 py-8 text-center text-[hsl(var(--muted-foreground))]">
                                        {t('common.noData')}
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            )}

            <LeavePolicyDialog
                open={isDialogOpen}
                onClose={(saved: boolean) => {
                    setIsDialogOpen(false);
                    if (saved) loadData();
                }}
                policy={selectedPolicy}
            />
        </div>
    );
}
