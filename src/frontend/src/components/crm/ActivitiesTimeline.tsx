import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Phone, Mail, Calendar, StickyNote, Plus, CheckCircle, Clock } from 'lucide-react';
import { api } from '@/services/api';
import { Button, Input, Modal, Badge, useToast, Select } from '@/components/ui';

interface Activity {
    id: string;
    type: number; // 0: Call, 1: Email, 2: Meeting, 3: Note
    subject: string;
    description: string | null;
    activityDate: string;
    isScheduled: boolean;
    isCompleted: boolean;
    completedAt: string | null;
    createdAt: string;
}

interface ActivitiesTimelineProps {
    entityId: string;
    entityType: 'Lead' | 'Opportunity' | 'BusinessPartner';
}

const ActivityTypeIcon = ({ type }: { type: number }) => {
    switch (type) {
        case 0: return <Phone className="w-4 h-4" />;
        case 1: return <Mail className="w-4 h-4" />;
        case 2: return <Calendar className="w-4 h-4" />;
        case 3: return <StickyNote className="w-4 h-4" />;
        default: return <Clock className="w-4 h-4" />;
    }
};

const ActivityTypeLabel = ({ type }: { type: number }) => {
    const { t } = useTranslation();
    const types = ['Call', 'Email', 'Meeting', 'Note'];
    return <span>{t(`activities.types.${types[type]}`) || types[type]}</span>;
};

export function ActivitiesTimeline({ entityId, entityType }: ActivitiesTimelineProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [activities, setActivities] = useState<Activity[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [formData, setFormData] = useState({
        type: '0',
        subject: '',
        description: '',
        activityDate: new Date().toISOString().slice(0, 16), // datetime-local format
        isScheduled: false
    });

    const loadActivities = useCallback(async () => {
        setIsLoading(true);
        const result = await api.activities.getAll(1, 50, entityId, entityType);
        if (result.success && result.data) {
            setActivities(result.data.data);
        }
        setIsLoading(false);
    }, [entityId, entityType]);

    useEffect(() => {
        loadActivities();
    }, [loadActivities]);

    const handleCreateWrapper = async (e: React.FormEvent) => {
        e.preventDefault(); // Prevent accidental form submissions if placed in a form
        handleCreate();
    };

    const handleCreate = async () => {
        if (!formData.subject) return;

        setIsSubmitting(true);
        try {
            const payload: any = {
                ...formData,
                type: parseInt(formData.type),
                activityDate: new Date(formData.activityDate).toISOString()
            };
            if (entityType === 'Lead') payload.leadId = entityId;
            else if (entityType === 'Opportunity') payload.opportunityId = entityId;
            else if (entityType === 'BusinessPartner') payload.partnerId = entityId;

            const result = await api.activities.create(payload);

            if (result.success) {
                toast.success(t('activities.created'));
                setIsModalOpen(false);
                setFormData({
                    type: '0',
                    subject: '',
                    description: '',
                    activityDate: new Date().toISOString().slice(0, 16),
                    isScheduled: false
                });
                loadActivities();
            } else {
                toast.error(t('common.error'), result.error);
            }
        } catch (err) {
            toast.error(t('common.error'), (err as Error).message);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleComplete = async (activity: Activity) => {
        try {
            const result = await api.activities.complete(activity.id);
            if (result.success) {
                toast.success(t('activities.completed'));
                loadActivities();
            }
        } catch (err) {
            toast.error(t('common.error'), (err as Error).message);
        }
    };

    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold">{t('activities.timeline')}</h3>
                <Button size="sm" onClick={() => setIsModalOpen(true)}>
                    <Plus className="w-4 h-4 mr-2" />
                    {t('activities.add')}
                </Button>
            </div>

            <div className="relative space-y-4 pl-4 border-l-2 border-gray-200 dark:border-gray-800">
                {isLoading ? (
                    <div className="py-4 text-center text-sm text-gray-500">
                        {t('common.loading', 'Loading...')}
                    </div>
                ) : activities.length === 0 ? (
                    <div className="py-4 text-center text-sm text-gray-500">
                        {t('activities.noActivities', 'No activities found')}
                    </div>
                ) : (
                    activities.map((activity) => (
                        <div key={activity.id} className="relative pb-4">
                            <div className={`absolute -left-[21px] top-1 w-8 h-8 rounded-full flex items-center justify-center border-2 ${activity.isCompleted
                                ? 'bg-green-100 border-green-500 text-green-600 dark:bg-green-900/30'
                                : 'bg-white border-gray-300 text-gray-500 dark:bg-gray-950 dark:border-gray-700'
                                }`}>
                                <ActivityTypeIcon type={activity.type} />
                            </div>

                            <div className="ml-4 bg-white dark:bg-gray-900 p-4 rounded-lg border shadow-sm">
                                <div className="flex items-start justify-between">
                                    <div>
                                        <div className="flex items-center gap-2">
                                            <h4 className="font-medium">{activity.subject}</h4>
                                            <Badge variant="default"><ActivityTypeLabel type={activity.type} /></Badge>
                                            {activity.isScheduled && !activity.isCompleted && (
                                                <Badge variant="warning">{t('activities.scheduled')}</Badge>
                                            )}
                                        </div>
                                        <p className="text-sm text-gray-500 mt-1">
                                            {new Date(activity.activityDate).toLocaleString()}
                                        </p>
                                        {activity.description && (
                                            <p className="mt-2 text-sm">{activity.description}</p>
                                        )}
                                    </div>
                                    {activity.isScheduled && !activity.isCompleted && (
                                        <Button
                                            size="sm"
                                            variant="ghost"
                                            className="text-green-600 hover:bg-green-50 dark:hover:bg-green-900/20"
                                            onClick={() => handleComplete(activity)}
                                        >
                                            <CheckCircle className="w-4 h-4" />
                                        </Button>
                                    )}
                                </div>
                            </div>
                        </div>
                    )))}
            </div>

            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={t('activities.newActivity')}
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button onClick={handleCreateWrapper} isLoading={isSubmitting}>
                            {t('common.save')}
                        </Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <Select
                        label={t('activities.type')}
                        value={formData.type}
                        onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                        options={[
                            { value: '0', label: t('activities.types.Call') || 'Call' },
                            { value: '1', label: t('activities.types.Email') || 'Email' },
                            { value: '2', label: t('activities.types.Meeting') || 'Meeting' },
                            { value: '3', label: t('activities.types.Note') || 'Note' },
                        ]}
                    />
                    <Input
                        label={t('activities.subject')}
                        value={formData.subject}
                        onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
                        required
                    />
                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            type="datetime-local"
                            label={t('activities.date')}
                            value={formData.activityDate}
                            onChange={(e) => setFormData({ ...formData, activityDate: e.target.value })}
                        />
                        <div className="flex items-center pt-8">
                            <label className="flex items-center gap-2 cursor-pointer">
                                <input
                                    type="checkbox"
                                    checked={formData.isScheduled}
                                    onChange={(e) => setFormData({ ...formData, isScheduled: e.target.checked })}
                                    className="rounded border-gray-300"
                                />
                                <span className="text-sm font-medium">{t('activities.isScheduled')}</span>
                            </label>
                        </div>
                    </div>
                    <div>
                        <label className="block text-sm font-medium mb-1">{t('activities.description')}</label>
                        <textarea
                            className="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                            rows={3}
                            value={formData.description}
                            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                        />
                    </div>
                </div>
            </Modal>
        </div>
    );
}
