import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Map, Users, AlertTriangle, UserX } from 'lucide-react';
import { api } from '@/services/api';
import { Button, Input, useToast } from '@/components/ui';

export function CrmSettingsPage() {
    const { t } = useTranslation();
    const [activeTab, setActiveTab] = useState('territories');

    const tabs = [
        { id: 'territories', label: t('crm.territories', 'Territories'), icon: Map },
        { id: 'competitors', label: t('crm.competitors', 'Competitors'), icon: Users },
        { id: 'lossReasons', label: t('crm.lossReasons', 'Loss Reasons'), icon: AlertTriangle },
        { id: 'rejectionReasons', label: t('crm.rejectionReasons', 'Rejection Reasons'), icon: UserX },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold tracking-tight">{t('crm.settings', 'CRM Settings')}</h1>
                    <p className="text-sm text-muted-foreground">
                        {t('crm.settingsSubtitle', 'Manage administrative lookup values and configuration')}
                    </p>
                </div>
            </div>

            <div className="flex flex-col md:flex-row gap-6">
                {/* Lateral Navigation */}
                <div className="w-full md:w-64 flex-shrink-0">
                    <nav className="flex space-x-2 md:flex-col md:space-x-0 md:space-y-1 overflow-x-auto pb-4 md:pb-0">
                        {tabs.map(tab => {
                            const Icon = tab.icon;
                            return (
                                <button
                                    key={tab.id}
                                    onClick={() => setActiveTab(tab.id)}
                                    className={`
                                        flex items-center space-x-3 px-4 py-3 text-sm font-medium rounded-xl transition-colors whitespace-nowrap
                                        ${activeTab === tab.id
                                            ? 'bg-primary text-primary-foreground shadow-sm'
                                            : 'text-muted-foreground hover:bg-muted/50 hover:text-foreground'
                                        }
                                    `}
                                >
                                    <Icon className="w-5 h-5 flex-shrink-0" />
                                    <span>{tab.label}</span>
                                </button>
                            );
                        })}
                    </nav>
                </div>

                {/* Content Area */}
                <div className="flex-1 bg-[hsl(var(--card))] rounded-xl border p-6 min-h-[500px]">
                    {activeTab === 'territories' && <LookupManager type="territories" title={t('crm.territories', 'Territories')} />}
                    {activeTab === 'competitors' && <LookupManager type="competitors" title={t('crm.competitors', 'Competitors')} />}
                    {activeTab === 'lossReasons' && <LookupManager type="lossReasons" title={t('crm.lossReasons', 'Loss Reasons')} />}
                    {activeTab === 'rejectionReasons' && <LookupManager type="rejectionReasons" title={t('crm.rejectionReasons', 'Rejection Reasons')} />}
                </div>
            </div>
        </div>
    );
}

// Reusable component for simple CRUD lookup tables
function LookupManager({ type, title }: { type: 'territories' | 'competitors' | 'lossReasons' | 'rejectionReasons', title: string }) {
    const { t } = useTranslation();
    const toast = useToast();
    const [items, setItems] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    // Simple inline form state
    const [newItemName, setNewItemName] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        loadData();
    }, [type]);

    const loadData = async () => {
        setLoading(true);
        try {
            const service = api[type] as any;
            const response = await service.getAll();
            if (response.success && response.data) {
                setItems(response.data.data || response.data);
            }
        } catch (error) {
            console.error('Failed to load data', error);
        } finally {
            setLoading(false);
        }
    };

    const handleAdd = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!newItemName.trim()) return;

        setIsSubmitting(true);
        try {
            const service = api[type] as any;
            const response = await service.create({ name: newItemName, isActive: true });

            if (response.success) {
                toast.success(t('common.saved', 'Saved successfully'));
                setNewItemName('');
                loadData();
            } else {
                toast.error(t('common.error'), response.error);
            }
        } catch (error: any) {
            toast.error(t('common.error'), error.message);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async (id: string) => {
        if (!window.confirm(t('common.deleteConfirmation'))) return;

        try {
            const service = api[type] as any;
            const response = await service.delete(id);
            if (response.success) {
                toast.success(t('common.deleted'));
                loadData();
            } else {
                toast.error(t('common.error'), response.error);
            }
        } catch (error: any) {
            toast.error(t('common.error'), error.message);
        }
    };

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-semibold">{title}</h3>
            </div>

            <form onSubmit={handleAdd} className="flex gap-4 items-end bg-slate-50 dark:bg-slate-900/50 p-4 rounded-lg border">
                <div className="flex-1">
                    <Input
                        label={t('common.name')}
                        value={newItemName}
                        onChange={e => setNewItemName(e.target.value)}
                        placeholder={t('common.enterName', 'Enter name...')}
                        required
                    />
                </div>
                <Button type="submit" isLoading={isSubmitting} disabled={!newItemName.trim()}>
                    {t('common.add')}
                </Button>
            </form>

            <div className="border rounded-lg overflow-hidden">
                <table className="min-w-full divide-y divide-border">
                    <thead className="bg-muted">
                        <tr>
                            <th className="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">{t('common.name')}</th>
                            <th className="px-6 py-3 text-right text-xs font-medium text-muted-foreground uppercase tracking-wider">{t('common.actions')}</th>
                        </tr>
                    </thead>
                    <tbody className="bg-[hsl(var(--card))] divide-y divide-border">
                        {loading ? (
                            <tr>
                                <td colSpan={2} className="px-6 py-8 text-center text-muted-foreground">
                                    {t('common.loading')}
                                </td>
                            </tr>
                        ) : items.length === 0 ? (
                            <tr>
                                <td colSpan={2} className="px-6 py-8 text-center text-muted-foreground">
                                    {t('common.noData')}
                                </td>
                            </tr>
                        ) : (
                            items.map(item => (
                                <tr key={item.id} className="hover:bg-muted/50 transition-colors">
                                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                                        {item.name}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                                        <Button variant="ghost" size="sm" onClick={() => handleDelete(item.id)} className="text-red-600 hover:text-red-700 hover:bg-red-50 dark:hover:bg-red-950/50">
                                            {t('common.delete')}
                                        </Button>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default CrmSettingsPage;
