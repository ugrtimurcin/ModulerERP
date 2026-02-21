import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Save, Phone, User } from 'lucide-react';
import { Button, Input, Select, useToast } from '@/components/ui';
import { api } from '@/services/api';
import { ActivitiesTimeline } from '@/components/crm/ActivitiesTimeline';

export function LeadDetail({ mode = 'view' }: { mode?: 'view' | 'create' | 'edit' }) {
    const { id } = useParams();
    const navigate = useNavigate();
    const { t } = useTranslation();
    const toast = useToast();

    const [loading, setLoading] = useState(mode !== 'create');
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Dropdown State
    const [territories, setTerritories] = useState<any[]>([]);

    // Tabs State
    const [activeTab, setActiveTab] = useState('overview');

    // Form State
    const [formData, setFormData] = useState<any>({
        title: '',
        firstName: '',
        lastName: '',
        company: '',
        email: '',
        phone: '',
        status: 'New',
        source: '',
        territoryId: '', // Phase 3 Field Alignment pre-work
        isMarketingConsentGiven: false,
        consentDate: null,
        consentSource: '',
    });

    useEffect(() => {
        loadDropdowns();
        if (mode !== 'create' && id) {
            loadLead(id);
        }
    }, [id, mode]);

    const loadDropdowns = async () => {
        try {
            const tResult = await api.territories.getAll();
            if (tResult.success && tResult.data?.data) {
                setTerritories(tResult.data.data);
            }
        } catch (error) {
            console.error("Failed to load dropdowns", error);
        }
    };

    const loadLead = async (leadId: string) => {
        try {
            // Note: Since api.leads.getById doesn't exist explicitly in api.ts, 
            // We retrieve from getAll or create a new endpoint.
            // Wait, leads Service has getAll, create, update, delete, convert.
            // Let's assume we add getById soon. But for now we simulate or fetch all.
            const response = await api.leads.getAll(1, 100);
            const lead = response.data?.data.find((l: any) => l.id === leadId);
            if (lead) {
                setFormData({
                    title: lead.title || '',
                    firstName: lead.firstName,
                    lastName: lead.lastName,
                    company: lead.company || '',
                    email: lead.email || '',
                    phone: lead.phone || '',
                    status: lead.status || 'New',
                    source: lead.source || '',
                    // Default missing values for now
                    territoryId: lead.territoryId || '',
                    isMarketingConsentGiven: lead.isMarketingConsentGiven || false,
                    consentDate: null,
                    consentSource: '',
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
            if (mode === 'create') {
                const response = await api.leads.create(formData);
                if (response.success) {
                    toast.success(t('leads.leadCreated'));
                    navigate('/leads');
                } else {
                    toast.error(t('common.error'), response.error);
                }
            } else if (id) {
                const response = await api.leads.update(id, formData);
                if (response.success) {
                    toast.success(t('leads.leadUpdated'));
                    // Wait, stay on page
                    loadLead(id);
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

    if (loading) return <div className="p-8 text-center text-muted-foreground">{t('common.loading')}</div>;

    const tabs = [
        { id: 'overview', label: t('common.overview') },
        ...(mode !== 'create' ? [
            { id: 'activities', label: t('activities.timeline') },
            { id: 'kvkk', label: t('crm.kvkk', 'KVKK & Consent') }
        ] : [
            { id: 'kvkk', label: t('crm.kvkk', 'KVKK & Consent') }
        ])
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                    <Button variant="ghost" size="icon" onClick={() => navigate('/leads')}>
                        <ArrowLeft className="h-4 w-4" />
                    </Button>
                    <div>
                        <h1 className="text-2xl font-bold tracking-tight">
                            {mode === 'create' ? t('leads.createLead') : `${formData.firstName} ${formData.lastName}`}
                        </h1>
                        <p className="text-sm text-muted-foreground">
                            {mode === 'create' ? t('leads.subtitle') : (formData.company || t('leads.status') + ': ' + formData.status)}
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
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <div className="grid gap-6 md:grid-cols-2">
                            {/* Personal Info */}
                            <div className="space-y-4">
                                <h3 className="font-semibold text-lg flex items-center gap-2">
                                    <User className="w-5 h-5 text-blue-500" />
                                    {t('leads.personalInfo', 'Personal Information')}
                                </h3>
                                <div className="grid grid-cols-2 gap-4">
                                    <Input
                                        label={t('common.title')}
                                        value={formData.title}
                                        onChange={e => setFormData({ ...formData, title: e.target.value })}
                                        placeholder="Mr, Mrs, Dr"
                                    />
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    <Input
                                        label={t('leads.firstName')}
                                        value={formData.firstName}
                                        onChange={e => setFormData({ ...formData, firstName: e.target.value })}
                                        required
                                    />
                                    <Input
                                        label={t('leads.lastName')}
                                        value={formData.lastName}
                                        onChange={e => setFormData({ ...formData, lastName: e.target.value })}
                                        required
                                    />
                                </div>
                                <Input
                                    label={t('leads.company')}
                                    value={formData.company}
                                    onChange={e => setFormData({ ...formData, company: e.target.value })}
                                />
                            </div>

                            {/* Contact Info */}
                            <div className="space-y-4">
                                <h3 className="font-semibold text-lg flex items-center gap-2">
                                    <Phone className="w-5 h-5 text-green-500" />
                                    {t('leads.contactInfo', 'Contact Information')}
                                </h3>
                                <Input
                                    label={t('leads.email')}
                                    type="email"
                                    value={formData.email}
                                    onChange={e => setFormData({ ...formData, email: e.target.value })}
                                />
                                <Input
                                    label={t('leads.phone')}
                                    value={formData.phone}
                                    onChange={e => setFormData({ ...formData, phone: e.target.value })}
                                />
                            </div>

                            {/* Lead Details */}
                            <div className="space-y-4 md:col-span-2 border-t pt-4">
                                <h3 className="font-semibold text-lg">
                                    {t('leads.leadDetails', 'Lead Details')}
                                </h3>
                                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                                    <Select
                                        label={t('leads.status')}
                                        value={formData.status}
                                        onChange={e => setFormData({ ...formData, status: e.target.value })}
                                        options={[
                                            { value: 'New', label: t('leads.New') },
                                            { value: 'Contacted', label: t('leads.Contacted') },
                                            { value: 'Qualified', label: t('leads.Qualified') },
                                            { value: 'Junk', label: t('leads.Junk') },
                                        ]}
                                    />
                                    <Input
                                        label={t('leads.source')}
                                        value={formData.source}
                                        onChange={e => setFormData({ ...formData, source: e.target.value })}
                                    />
                                    {/* Territory placeholder */}
                                    <Select
                                        label={t('crm.territory', 'Territory')}
                                        value={formData.territoryId}
                                        onChange={e => setFormData({ ...formData, territoryId: e.target.value })}
                                        options={[
                                            { value: '', label: t('common.none') },
                                            ...territories.map(t => ({ value: t.id, label: t.name }))
                                        ]}
                                    />
                                </div>
                            </div>
                        </div>
                    </div>
                )}

                {activeTab === 'activities' && id && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <ActivitiesTimeline entityId={id} entityType="Lead" />
                    </div>
                )}

                {activeTab === 'kvkk' && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <h3 className="font-semibold text-lg mb-4">{t('crm.kvkk', 'KVKK & Consent')}</h3>
                        <div className="space-y-4 max-w-lg">
                            <label className="flex items-center gap-3 cursor-pointer p-3 border rounded-lg hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors">
                                <input
                                    type="checkbox"
                                    checked={formData.isMarketingConsentGiven}
                                    onChange={(e) => setFormData({ ...formData, isMarketingConsentGiven: e.target.checked })}
                                    className="w-5 h-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                                />
                                <div>
                                    <p className="font-medium text-sm">{t('crm.isMarketingConsentGiven', 'Marketing Consent Given')}</p>
                                    <p className="text-xs text-muted-foreground">{t('crm.consentDescription', 'Indicates if the lead has legally agreed to receive promotional communications.')}</p>
                                </div>
                            </label>

                            {formData.isMarketingConsentGiven && (
                                <div className="grid grid-cols-2 gap-4 mt-4 animate-in fade-in slide-in-from-top-2">
                                    <Input
                                        label={t('crm.consentSource', 'Consent Source')}
                                        value={formData.consentSource}
                                        onChange={e => setFormData({ ...formData, consentSource: e.target.value })}
                                        placeholder="e.g. Website Form, Event"
                                    />
                                    <Input
                                        label={t('crm.consentDate', 'Consent Date')}
                                        type="date"
                                        value={formData.consentDate ? formData.consentDate.split('T')[0] : ''}
                                        onChange={e => setFormData({ ...formData, consentDate: e.target.value })}
                                    />
                                </div>
                            )}
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}

export default LeadDetail;
