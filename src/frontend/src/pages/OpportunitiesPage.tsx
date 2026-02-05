import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, TrendingUp, Building2 } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Badge, useToast, useDialog, Select } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Opportunity {
    id: string;
    title: string;
    partnerId: string | null;
    partnerName: string | null;
    estimatedValue: number;
    currencyCode: string;
    stage: 'Discovery' | 'Proposal' | 'Negotiation' | 'Won' | 'Lost';
    probability: number;
    weightedValue: number;
    expectedCloseDate: string | null;
    assignedUserId: string | null;
    isActive: boolean;
    createdAt: string;
}

interface Partner {
    id: string;
    name: string;
}

interface Currency {
    id: string;
    code: string;
    name: string;
}

interface User {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
}

interface OpportunityFormData {
    title: string;
    partnerId: string;
    leadId: string;
    currencyId: string;
    estimatedValue: number;
    stage: string;
    probability: number;
    expectedCloseDate: string;
    assignedUserId: string;
}

export function OpportunitiesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [opportunities, setOpportunities] = useState<Opportunity[]>([]);
    const [partners, setPartners] = useState<Partner[]>([]);
    const [currencies, setCurrencies] = useState<Currency[]>([]);
    const [users, setUsers] = useState<User[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingOpportunity, setEditingOpportunity] = useState<Opportunity | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Form state
    const [formData, setFormData] = useState<OpportunityFormData>({
        title: '',
        partnerId: '',
        leadId: '',
        currencyId: '',
        estimatedValue: 0,
        stage: 'Discovery',
        probability: 10,
        expectedCloseDate: '',
        assignedUserId: '',
    });
    const [formErrors, setFormErrors] = useState<Partial<OpportunityFormData>>({});

    const loadOpportunities = useCallback(async () => {
        setIsLoading(true);
        const result = await api.opportunities.getAll(page, pageSize);
        if (result.success && result.data) {
            setOpportunities(result.data.data as Opportunity[]);
            setTotal(result.data.totalCount);
        }
        setIsLoading(false);
    }, [page]);

    const loadDropdowns = async () => {
        const [pResult, cResult, uResult] = await Promise.all([
            api.partners.getAll(1, 1000),
            api.getActiveCurrencies(),
            api.users.getAll(1, 1000)
        ]);

        if (pResult.success && pResult.data) {
            setPartners(pResult.data.data.map((p: any) => ({ id: p.id, name: p.name })));
        }
        if (cResult.success && cResult.data) {
            setCurrencies(cResult.data);
        }
        if (uResult.success && uResult.data) {
            setUsers(uResult.data.data.map((u: any) => ({
                id: u.id,
                firstName: u.firstName,
                lastName: u.lastName,
                email: u.email
            })));
        }
    };

    useEffect(() => {
        loadOpportunities();
        loadDropdowns();
    }, [loadOpportunities]);

    const openCreateModal = () => {
        setEditingOpportunity(null);
        setFormData({
            title: '',
            partnerId: '',
            leadId: '',
            // Default to TRY or first available currency
            currencyId: currencies.find(c => c.code === 'TRY')?.id || currencies[0]?.id || '',
            estimatedValue: 0,
            stage: 'Discovery',
            probability: 10,
            expectedCloseDate: '',
            assignedUserId: '',
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = (opp: Opportunity) => {
        setEditingOpportunity(opp);
        setFormData({
            title: opp.title,
            partnerId: opp.partnerId || '',
            leadId: '',
            currencyId: currencies.find(c => c.code === opp.currencyCode)?.id || '',
            estimatedValue: opp.estimatedValue,
            stage: opp.stage,
            probability: opp.probability,
            expectedCloseDate: opp.expectedCloseDate ? opp.expectedCloseDate.split('T')[0] : '',
            assignedUserId: opp.assignedUserId || '',
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const validateForm = (): boolean => {
        const errors: Partial<OpportunityFormData> = {};

        if (!formData.title.trim()) {
            errors.title = t('common.required');
        }

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async () => {
        if (!validateForm()) return;

        setIsSubmitting(true);
        try {
            const payload: any = { ...formData };
            if (!payload.partnerId) delete payload.partnerId;
            if (!payload.leadId) delete payload.leadId;
            if (!payload.expectedCloseDate) delete payload.expectedCloseDate;

            if (editingOpportunity) {
                const result = await api.opportunities.update(editingOpportunity.id, payload);
                if (result.success) {
                    toast.success(t('opportunities.opportunityUpdated'));
                    setIsModalOpen(false);
                    loadOpportunities();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.opportunities.create(payload);
                if (result.success) {
                    toast.success(t('opportunities.opportunityCreated'));
                    setIsModalOpen(false);
                    loadOpportunities();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            }
        } catch (err) {
            toast.error(t('common.error'), (err as Error).message);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async (opp: Opportunity) => {
        const confirmed = await dialog.danger({
            title: t('opportunities.deleteOpportunity'),
            message: t('opportunities.confirmDeleteOpportunity'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.opportunities.delete(opp.id);
            if (result.success) {
                toast.success(t('opportunities.opportunityDeleted'));
                loadOpportunities();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const handleStageChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const stage = e.target.value;
        let probability = formData.probability;
        switch (stage) {
            case 'Discovery': probability = 10; break;
            case 'Proposal': probability = 30; break;
            case 'Negotiation': probability = 60; break;
            case 'Won': probability = 100; break;
            case 'Lost': probability = 0; break;
        }
        setFormData({ ...formData, stage, probability });
    };

    const getStageBadge = (stage: string) => {
        const variants: Record<string, "info" | "success" | "warning" | "error" | "default"> = {
            Discovery: "info",
            Proposal: "warning",
            Negotiation: "default",
            Won: "success",
            Lost: "error"
        };
        return <Badge variant={variants[stage] || "default"}>{t(`opportunities.${stage}`)}</Badge>;
    };

    const columns: Column<Opportunity>[] = [
        {
            key: 'title',
            header: t('opportunities.title_field'),
            render: (opp) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-full bg-purple-100 dark:bg-purple-900/30 text-purple-600 flex items-center justify-center">
                        <TrendingUp className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-medium">{opp.title}</p>
                        {opp.partnerName && (
                            <div className="flex items-center gap-1 text-sm text-[hsl(var(--muted-foreground))]">
                                <Building2 className="w-3 h-3" />
                                <span>{opp.partnerName}</span>
                            </div>
                        )}
                    </div>
                </div>
            ),
        },
        {
            key: 'value',
            header: t('opportunities.value'),
            render: (opp) => (
                <div className="font-medium">
                    {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(opp.estimatedValue)}
                    <div className="text-xs text-[hsl(var(--muted-foreground))]">
                        {t('opportunities.weightedValue')}: {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(opp.weightedValue)}
                    </div>
                </div>
            ),
        },
        {
            key: 'stage',
            header: t('opportunities.stage'),
            render: (opp) => (
                <div className="flex flex-col items-start gap-1">
                    {getStageBadge(opp.stage)}
                    <span className="text-xs text-[hsl(var(--muted-foreground))]">{opp.probability}%</span>
                </div>
            ),
        },
        {
            key: 'date',
            header: t('opportunities.expectedCloseDate'),
            render: (opp) => (
                <div className="text-sm">
                    {opp.expectedCloseDate ? new Date(opp.expectedCloseDate).toLocaleDateString() : '-'}
                </div>
            )
        }
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('opportunities.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('opportunities.subtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('opportunities.createOpportunity')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={opportunities}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                pagination={{
                    page,
                    pageSize,
                    total,
                    onPageChange: setPage,
                }}
                actions={(opp) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => openEditModal(opp)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>

                        <button
                            onClick={() => handleDelete(opp)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            {/* Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingOpportunity ? t('opportunities.editOpportunity') : t('opportunities.createOpportunity')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button onClick={handleSubmit} isLoading={isSubmitting}>
                            {t('common.save')}
                        </Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <Input
                        label={t('opportunities.title_field')}
                        value={formData.title}
                        onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                        error={formErrors.title}
                        required
                    />

                    <div className="space-y-2">
                        <label className="text-sm font-medium">{t('opportunities.partner')}</label>
                        <select
                            className="w-full h-10 px-3 py-2 rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-[hsl(var(--primary))]"
                            value={formData.partnerId}
                            onChange={(e) => setFormData({ ...formData, partnerId: e.target.value })}
                        >
                            <option value="">{t('common.select')}</option>
                            {partners.map(p => (
                                <option key={p.id} value={p.id}>{p.name}</option>
                            ))}
                        </select>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <Select
                                label={t('opportunities.currency')}
                                value={formData.currencyId}
                                onChange={(e) => setFormData({ ...formData, currencyId: e.target.value })}
                                options={currencies.map(c => ({ value: c.id, label: `${c.code} (${c.name})` }))}
                            />
                        </div>
                        <Input
                            label={t('opportunities.value')}
                            type="number"
                            value={formData.estimatedValue}
                            onChange={(e) => setFormData({ ...formData, estimatedValue: parseFloat(e.target.value) })}
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <Select
                            label={t('opportunities.assignedUser')}
                            value={formData.assignedUserId}
                            onChange={(e) => setFormData({ ...formData, assignedUserId: e.target.value })}
                            options={users.map(u => ({ value: u.id, label: `${u.firstName} ${u.lastName}` }))}
                        />
                        <Input
                            label={t('opportunities.expectedCloseDate')}
                            type="date"
                            value={formData.expectedCloseDate}
                            onChange={(e) => setFormData({ ...formData, expectedCloseDate: e.target.value })}
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <Select
                            label={t('opportunities.stage')}
                            value={formData.stage}
                            onChange={handleStageChange}
                            options={[
                                { value: 'Discovery', label: t('opportunities.Discovery') },
                                { value: 'Proposal', label: t('opportunities.Proposal') },
                                { value: 'Negotiation', label: t('opportunities.Negotiation') },
                                { value: 'Won', label: t('opportunities.Won') },
                                { value: 'Lost', label: t('opportunities.Lost') },
                            ]}
                        />
                        <Input
                            label={t('opportunities.probability')}
                            type="number"
                            min={0}
                            max={100}
                            value={formData.probability}
                            onChange={(e) => setFormData({ ...formData, probability: parseInt(e.target.value) })}
                        />
                    </div>
                </div>
            </Modal >
        </div >
    );
}
