import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2 } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { api } from '@/lib/api';
import { SgkRiskProfileDialog } from './SgkRiskProfileDialog';

interface SgkRiskProfile {
    id: string;
    name: string;
    description?: string;
    employeePremiumRate: number;
    employerPremiumRate: number;
    unemploymentEmployeeRate: number;
    unemploymentEmployerRate: number;
    isActive: boolean;
}

export function SgkRiskProfilesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const [profiles, setProfiles] = useState<SgkRiskProfile[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [selectedProfile, setSelectedProfile] = useState<SgkRiskProfile | null>(null);

    const loadData = async () => {
        setIsLoading(true);
        try {
            const res = await api.get('/hr/sgk-risk-profiles');
            const data = (res as any).data || res;
            setProfiles(Array.isArray(data) ? data : data.items || []);
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
            await api.delete(`/hr/sgk-risk-profiles/${id}`);
            toast.success(t('common.deleted'));
            loadData();
        } catch (error) {
            toast.error(t('common.error'));
        }
    };

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h2 className="text-xl font-semibold">{t('hr.sgkRiskProfiles', 'SGK Risk Profiles')}</h2>
                    <p className="text-sm text-[hsl(var(--muted-foreground))]">Manage employer and employee premium rates.</p>
                </div>
                <Button onClick={() => { setSelectedProfile(null); setIsDialogOpen(true); }}>
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
                                <th className="px-4 py-3">Premium (Emp/Empr)</th>
                                <th className="px-4 py-3">Unemp (Emp/Empr)</th>
                                <th className="px-4 py-3">Status</th>
                                <th className="px-4 py-3 text-right">{t('common.actions')}</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-[hsl(var(--border))]">
                            {profiles.map(item => (
                                <tr key={item.id} className="hover:bg-[hsl(var(--muted)/0.5)]">
                                    <td className="px-4 py-3 font-medium">{item.name}</td>
                                    <td className="px-4 py-3 text-[hsl(var(--muted-foreground))]">
                                        <span className="text-indigo-600 font-medium">{item.employeePremiumRate.toFixed(2)}%</span> / {item.employerPremiumRate.toFixed(2)}%
                                    </td>
                                    <td className="px-4 py-3 text-[hsl(var(--muted-foreground))]">
                                        <span className="text-indigo-600 font-medium">{item.unemploymentEmployeeRate.toFixed(2)}%</span> / {item.unemploymentEmployerRate.toFixed(2)}%
                                    </td>
                                    <td className="px-4 py-3">
                                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${item.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                                            {item.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3 text-right">
                                        <Button variant="ghost" className="h-8 w-8 p-0" onClick={() => { setSelectedProfile(item); setIsDialogOpen(true); }}>
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

            <SgkRiskProfileDialog
                open={isDialogOpen}
                onClose={(saved: boolean) => { setIsDialogOpen(false); if (saved) loadData(); }}
                data={selectedProfile}
            />
        </div>
    );
}
