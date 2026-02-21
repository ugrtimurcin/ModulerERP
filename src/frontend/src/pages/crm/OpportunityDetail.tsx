import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Save, TrendingUp, FileText, Target } from 'lucide-react';
import { Button, Input, Select, useToast } from '@/components/ui';
import { api } from '@/services/api';
import { ActivitiesTimeline } from '@/components/crm/ActivitiesTimeline';

export function OpportunityDetail({ mode = 'view' }: { mode?: 'view' | 'create' | 'edit' }) {
    const { id } = useParams();
    const navigate = useNavigate();
    const { t } = useTranslation();
    const toast = useToast();

    const [loading, setLoading] = useState(mode !== 'create');
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Tabs State
    const [activeTab, setActiveTab] = useState('overview');

    // Data State
    const [partners, setPartners] = useState<any[]>([]);
    const [currencies, setCurrencies] = useState<any[]>([]);
    const [users, setUsers] = useState<any[]>([]);
    const [territories, setTerritories] = useState<any[]>([]);
    const [competitors, setCompetitors] = useState<any[]>([]);
    const [lossReasons, setLossReasons] = useState<any[]>([]);

    // Form State
    const [formData, setFormData] = useState<any>({
        title: '',
        partnerId: '',
        leadId: '',
        currencyId: '',
        estimatedValue: 0,
        stage: 'Discovery',
        probability: 10,
        expectedCloseDate: '',
        assignedUserId: '',
        territoryId: '', // Phase 3
        competitorId: '', // Phase 3
        lossReasonId: '' // Phase 3
    });

    useEffect(() => {
        loadDropdowns();
        if (mode !== 'create' && id) {
            loadOpportunity(id);
        }
    }, [id, mode]);

    const loadDropdowns = async () => {
        try {
            const [pResult, cResult, uResult, tResult, compResult, lrResult] = await Promise.all([
                api.partners.getAll(1, 1000),
                api.getActiveCurrencies(),
                api.users.getAll(1, 100),
                api.territories.getAll(),
                api.competitors.getAll(),
                api.lossReasons.getAll()
            ]);

            if (pResult.success && pResult.data) {
                setPartners(pResult.data.data);
            }
            if (cResult.success && cResult.data) {
                setCurrencies(cResult.data);
                if (mode === 'create') {
                    setFormData((prev: any) => ({
                        ...prev,
                        currencyId: cResult.data!.find((c: any) => c.code === 'TRY')?.id || cResult.data![0]?.id || ''
                    }));
                }
            }
            if (uResult.success && uResult.data) {
                setUsers(uResult.data.data);
            }
            if (tResult.success && tResult.data?.data) {
                setTerritories(tResult.data.data);
            }
            if (compResult.success && compResult.data?.data) {
                setCompetitors(compResult.data.data);
            }
            if (lrResult.success && lrResult.data?.data) {
                setLossReasons(lrResult.data.data);
            }
        } catch (error) {
            console.error("Failed to load dropdowns", error);
        }
    };

    const loadOpportunity = async (oppId: string) => {
        try {
            // Simulated fetch since getById might not exist yet
            const response = await api.opportunities.getAll(1, 100);
            const opp = response.data?.data.find((p: any) => p.id === oppId) as any;
            if (opp) {
                setFormData({
                    title: opp.title || '',
                    partnerId: opp.partnerId || '',
                    leadId: opp.leadId || '',
                    // Note: API returns currencyCode in list view, need to map to ID from dropdowns later
                    currencyId: '',
                    estimatedValue: opp.estimatedValue || 0,
                    stage: opp.stage || 'Discovery',
                    probability: opp.probability || 10,
                    expectedCloseDate: opp.expectedCloseDate ? opp.expectedCloseDate.split('T')[0] : '',
                    assignedUserId: opp.assignedUserId || '',
                    territoryId: opp.territoryId || '',
                    competitorId: opp.competitorId || '',
                    lossReasonId: opp.lossReasonId || ''
                });
            }
        } catch (error: any) {
            toast.error(t('common.error'), error.message);
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        setIsSubmitting(true);
        try {
            const payload = { ...formData };
            if (!payload.partnerId) delete payload.partnerId;
            if (!payload.leadId) delete payload.leadId;
            if (!payload.expectedCloseDate) delete payload.expectedCloseDate;

            if (mode === 'create') {
                const response = await api.opportunities.create(payload);
                if (response.success) {
                    toast.success(t('opportunities.opportunityCreated'));
                    navigate('/opportunities');
                } else {
                    toast.error(t('common.error'), response.error);
                }
            } else if (id) {
                const response = await api.opportunities.update(id, payload);
                if (response.success) {
                    toast.success(t('opportunities.opportunityUpdated'));
                    loadOpportunity(id);
                } else {
                    toast.error(t('common.error'), response.error);
                }
            }
        } catch (error: any) {
            toast.error(t('common.error'), error.message);
        } finally {
            setIsSubmitting(false);
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

    if (loading) return <div className="p-8 text-center text-muted-foreground">{t('common.loading')}</div>;

    const tabs = [
        { id: 'overview', label: t('common.overview') },
        ...(mode !== 'create' ? [
            { id: 'activities', label: t('activities.timeline') },
            { id: 'quotes', label: t('crm.quotes', 'Sales Quotes') },
            { id: 'competitors', label: t('crm.competitors', 'Competitors') }
        ] : [])
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                    <Button variant="ghost" size="icon" onClick={() => navigate('/opportunities')}>
                        <ArrowLeft className="h-4 w-4" />
                    </Button>
                    <div>
                        <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
                            <TrendingUp className="h-6 w-6 text-purple-600" />
                            {mode === 'create' ? t('opportunities.createOpportunity') : formData.title}
                        </h1>
                        <p className="text-sm text-muted-foreground">
                            {mode === 'create' ? t('opportunities.subtitle') : (formData.stage || t('opportunities.role'))}
                        </p>
                    </div>
                </div>
                <div className="flex space-x-2">
                    <Button onClick={handleSave} isLoading={isSubmitting}>
                        <Save className="mr-2 h-4 w-4" />
                        {t('common.save')}
                    </Button>
                </div>
            </div>

            {/* Tabs List */}
            <div className="border-b">
                <div className="flex space-x-4">
                    {tabs.map(tab => (
                        <button
                            key={tab.id}
                            className={`py-2 px-4 text-sm font-medium border-b-2 transition-colors ${activeTab === tab.id ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-foreground'}`}
                            onClick={() => setActiveTab(tab.id)}
                        >
                            {tab.label}
                        </button>
                    ))}
                </div>
            </div>

            {/* Tab Contents */}
            <div className="mt-6">
                {activeTab === 'overview' && (
                    <div className="grid gap-6 md:grid-cols-2">
                        {/* Core Details */}
                        <div className="bg-[hsl(var(--card))] rounded-xl border p-6 space-y-4">
                            <h3 className="font-semibold text-lg flex items-center gap-2">
                                <FileText className="w-5 h-5 text-blue-500" />
                                {t('common.overview')}
                            </h3>
                            <Input
                                label={t('opportunities.title_field')}
                                value={formData.title}
                                onChange={e => setFormData({ ...formData, title: e.target.value })}
                                required
                            />

                            <Select
                                label={t('opportunities.partner')}
                                value={formData.partnerId}
                                onChange={e => setFormData({ ...formData, partnerId: e.target.value })}
                                options={[
                                    { value: '', label: t('common.select') },
                                    ...partners.map(p => ({ value: p.id, label: p.name }))
                                ]}
                            />

                            <div className="grid grid-cols-2 gap-4">
                                <Select
                                    label={t('opportunities.currency')}
                                    value={formData.currencyId}
                                    onChange={e => setFormData({ ...formData, currencyId: e.target.value })}
                                    options={[
                                        { value: '', label: t('common.select') },
                                        ...currencies.map(c => ({ value: c.id, label: `${c.code} (${c.name})` }))
                                    ]}
                                />
                                <Input
                                    label={t('opportunities.value')}
                                    type="number"
                                    value={formData.estimatedValue}
                                    onChange={e => setFormData({ ...formData, estimatedValue: parseFloat(e.target.value) })}
                                />
                            </div>

                            {/* Target Details Phase 3 */}
                            <Select
                                label={t('crm.territory', 'Territory')}
                                value={formData.territoryId}
                                onChange={e => setFormData({ ...formData, territoryId: e.target.value })}
                                options={[
                                    { value: '', label: t('common.none') },
                                    ...territories.map(t => ({ value: t.id, label: t.name }))
                                ]}
                            />
                            <Select
                                label={t('crm.competitor', 'Competitor')}
                                value={formData.competitorId}
                                onChange={e => setFormData({ ...formData, competitorId: e.target.value })}
                                options={[
                                    { value: '', label: t('common.none') },
                                    ...competitors.map(c => ({ value: c.id, label: c.name }))
                                ]}
                            />
                        </div>

                        {/* Status & Assignment */}
                        <div className="bg-[hsl(var(--card))] rounded-xl border p-6 space-y-4">
                            <h3 className="font-semibold text-lg flex items-center gap-2">
                                <Target className="w-5 h-5 text-red-500" />
                                {t('common.status')}
                            </h3>
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
                                    onChange={e => setFormData({ ...formData, probability: parseInt(e.target.value) })}
                                />
                            </div>

                            <Input
                                label={t('opportunities.expectedCloseDate')}
                                type="date"
                                value={formData.expectedCloseDate}
                                onChange={e => setFormData({ ...formData, expectedCloseDate: e.target.value })}
                            />

                            <Select
                                label={t('opportunities.assignedUser')}
                                value={formData.assignedUserId}
                                onChange={e => setFormData({ ...formData, assignedUserId: e.target.value })}
                                options={[
                                    { value: '', label: t('common.unassigned') },
                                    ...users.map(u => ({ value: u.id, label: `${u.firstName} ${u.lastName}` }))
                                ]}
                            />

                            {formData.stage === 'Lost' && (
                                <Select
                                    label={t('crm.lossReason', 'Loss Reason')}
                                    value={formData.lossReasonId}
                                    onChange={e => setFormData({ ...formData, lossReasonId: e.target.value })}
                                    options={[
                                        { value: '', label: t('common.none') },
                                        ...lossReasons.map(lr => ({ value: lr.id, label: lr.name }))
                                    ]}
                                />
                            )}
                        </div>
                    </div>
                )}

                {activeTab === 'activities' && id && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <ActivitiesTimeline entityId={id} entityType="Opportunity" />
                    </div>
                )}

                {activeTab === 'quotes' && id && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <div className="flex justify-between items-center mb-4">
                            <h3 className="text-lg font-semibold">{t('crm.quotes', 'Sales Quotes')}</h3>
                            <Button variant="outline" size="sm" disabled>{t('common.create')}</Button>
                        </div>
                        <div className="text-center py-8 text-muted-foreground border-2 border-dashed rounded-lg">
                            {t('common.noData')}
                        </div>
                    </div>
                )}

                {activeTab === 'competitors' && id && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <div className="flex justify-between items-center mb-4">
                            <h3 className="text-lg font-semibold">{t('crm.competitors', 'Competitors')}</h3>
                            <Button variant="outline" size="sm" disabled>{t('common.add')}</Button>
                        </div>
                        <div className="text-center py-8 text-muted-foreground border-2 border-dashed rounded-lg">
                            {t('common.noData')}
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}

export default OpportunityDetail;
