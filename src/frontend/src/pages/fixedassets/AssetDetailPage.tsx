import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Package, UserCheck, UserMinus, Gauge, AlertTriangle, Wrench, Trash2, RefreshCw } from 'lucide-react';
import { Button, Badge, useToast } from '@/components/ui';
import { api } from '@/lib/api';
import { AssetAssignDialog } from './AssetAssignDialog';
import { AssetReturnDialog } from './AssetReturnDialog';
import { AssetMeterLogDialog } from './AssetMeterLogDialog';
import { AssetIncidentDialog } from './AssetIncidentDialog';
import { AssetMaintenanceDialog } from './AssetMaintenanceDialog';
import { AssetDisposeDialog } from './AssetDisposeDialog';

interface Asset {
    id: string;
    assetCode: string;
    name: string;
    description: string | null;
    categoryId: string;
    categoryName: string;
    status: number;
    acquisitionDate: string;
    acquisitionCost: number;
    salvageValue: number;
    accumulatedDepreciation: number;
    bookValue: number;
    serialNumber: string | null;
    barCode: string | null;
    assignedEmployeeId: string | null;
    assignedEmployeeName: string | null;
    currentMeter: number;
}

interface Assignment {
    id: string;
    employeeName: string;
    assignedDate: string;
    returnedDate: string | null;
    startMeterValue: number;
    endMeterValue: number | null;
    startCondition: string;
    endCondition: string | null;
}

interface MeterLog {
    id: string;
    logDate: string;
    meterValue: number;
    source: number;
}

interface Incident {
    id: string;
    incidentDate: string;
    description: string;
    status: number;
    isUserFault: boolean | null;
    deductFromSalary: boolean;
    deductionAmount: number | null;
}

interface Maintenance {
    id: string;
    serviceDate: string;
    supplierName: string;
    cost: number;
    description: string;
    nextServiceDate: string | null;
}

interface Disposal {
    id: string;
    disposalDate: string;
    type: number;
    saleAmount: number | null;
    bookValueAtDate: number;
    profitLoss: number;
}

const AssetStatus: Record<number, { label: string; variant: 'success' | 'warning' | 'error' | 'default' }> = {
    0: { label: 'In Stock', variant: 'default' },
    1: { label: 'Assigned', variant: 'success' },
    2: { label: 'Under Maintenance', variant: 'warning' },
    3: { label: 'Disposed', variant: 'error' },
    4: { label: 'Scrapped', variant: 'error' },
    5: { label: 'Sold', variant: 'default' },
};

export function AssetDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { t } = useTranslation();
    const toast = useToast();

    const [asset, setAsset] = useState<Asset | null>(null);
    const [assignments, setAssignments] = useState<Assignment[]>([]);
    const [meterLogs, setMeterLogs] = useState<MeterLog[]>([]);
    const [incidents, setIncidents] = useState<Incident[]>([]);
    const [maintenances, setMaintenances] = useState<Maintenance[]>([]);
    const [disposals, setDisposals] = useState<Disposal[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [activeTab, setActiveTab] = useState('overview');

    // Dialog states
    const [assignDialogOpen, setAssignDialogOpen] = useState(false);
    const [returnDialogOpen, setReturnDialogOpen] = useState(false);
    const [meterDialogOpen, setMeterDialogOpen] = useState(false);
    const [incidentDialogOpen, setIncidentDialogOpen] = useState(false);
    const [maintenanceDialogOpen, setMaintenanceDialogOpen] = useState(false);
    const [disposeDialogOpen, setDisposeDialogOpen] = useState(false);

    const loadAsset = useCallback(async () => {
        if (!id) return;
        setIsLoading(true);
        try {
            const [assetRes, assignRes, meterRes, incidentRes, maintRes, dispRes] = await Promise.all([
                api.get<Asset>(`/fixedassets/assets/${id}`),
                api.get<Assignment[]>(`/fixedassets/assets/${id}/assignments`),
                api.get<MeterLog[]>(`/fixedassets/assets/${id}/meter-logs`),
                api.get<Incident[]>(`/fixedassets/assets/${id}/incidents`),
                api.get<Maintenance[]>(`/fixedassets/assets/${id}/maintenances`),
                api.get<Disposal[]>(`/fixedassets/assets/${id}/disposals`),
            ]);

            setAsset(assetRes);
            setAssignments(assignRes);
            setMeterLogs(meterRes);
            setIncidents(incidentRes);
            setMaintenances(maintRes);
            setDisposals(dispRes);
        } catch (error) {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [id, toast, t]);

    useEffect(() => {
        loadAsset();
    }, [loadAsset]);

    const handleDialogClose = (saved: boolean) => {
        setAssignDialogOpen(false);
        setReturnDialogOpen(false);
        setMeterDialogOpen(false);
        setIncidentDialogOpen(false);
        setMaintenanceDialogOpen(false);
        setDisposeDialogOpen(false);
        if (saved) loadAsset();
    };

    const getStatusBadge = (status: number) => {
        const config = AssetStatus[status] || { label: 'Unknown', variant: 'default' as const };
        return <Badge variant={config.variant}>{t(`fixedAssets.status.${status}`) || config.label}</Badge>;
    };

    const tabs = [
        { key: 'overview', label: t('fixedAssets.overview') },
        { key: 'assignments', label: t('fixedAssets.assignments'), count: assignments.length },
        { key: 'meterLogs', label: t('fixedAssets.meterLogs'), count: meterLogs.length },
        { key: 'incidents', label: t('fixedAssets.incidents'), count: incidents.length },
        { key: 'maintenance', label: t('fixedAssets.maintenance'), count: maintenances.length },
        { key: 'disposals', label: t('fixedAssets.disposals'), count: disposals.length },
    ];

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-64">
                <RefreshCw className="w-8 h-8 animate-spin text-indigo-500" />
            </div>
        );
    }

    if (!asset) {
        return (
            <div className="text-center py-12">
                <p className="text-[hsl(var(--muted-foreground))]">{t('fixedAssets.assetNotFound')}</p>
            </div>
        );
    }

    const isAssigned = asset.status === 1;
    const isDisposed = asset.status === 3 || asset.status === 4 || asset.status === 5;

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                    <button
                        onClick={() => navigate('/fixed-assets/assets')}
                        className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                    >
                        <ArrowLeft className="w-5 h-5" />
                    </button>
                    <div className="flex items-center gap-3">
                        <div className="w-12 h-12 rounded-lg bg-gradient-to-br from-amber-500 to-orange-500 flex items-center justify-center text-white">
                            <Package className="w-6 h-6" />
                        </div>
                        <div>
                            <h1 className="text-2xl font-bold">{asset.name}</h1>
                            <p className="text-[hsl(var(--muted-foreground))] font-mono">{asset.assetCode}</p>
                        </div>
                    </div>
                    {getStatusBadge(asset.status)}
                </div>

                {/* Action Buttons */}
                {!isDisposed && (
                    <div className="flex items-center gap-2">
                        {!isAssigned ? (
                            <Button onClick={() => setAssignDialogOpen(true)}>
                                <UserCheck className="w-4 h-4" />
                                {t('fixedAssets.assign')}
                            </Button>
                        ) : (
                            <Button variant="secondary" onClick={() => setReturnDialogOpen(true)}>
                                <UserMinus className="w-4 h-4" />
                                {t('fixedAssets.return')}
                            </Button>
                        )}
                        <Button variant="secondary" onClick={() => setMeterDialogOpen(true)}>
                            <Gauge className="w-4 h-4" />
                            {t('fixedAssets.logMeter')}
                        </Button>
                        <Button variant="secondary" onClick={() => setIncidentDialogOpen(true)}>
                            <AlertTriangle className="w-4 h-4" />
                            {t('fixedAssets.reportIncident')}
                        </Button>
                        <Button variant="secondary" onClick={() => setMaintenanceDialogOpen(true)}>
                            <Wrench className="w-4 h-4" />
                            {t('fixedAssets.recordMaintenance')}
                        </Button>
                        <Button variant="danger" onClick={() => setDisposeDialogOpen(true)}>
                            <Trash2 className="w-4 h-4" />
                            {t('fixedAssets.dispose')}
                        </Button>
                    </div>
                )}
            </div>

            {/* Tabs */}
            <div className="border-b border-[hsl(var(--border))]">
                <nav className="flex gap-4">
                    {tabs.map((tab) => (
                        <button
                            key={tab.key}
                            onClick={() => setActiveTab(tab.key)}
                            className={`py-3 px-1 border-b-2 font-medium text-sm transition-colors ${activeTab === tab.key
                                ? 'border-indigo-500 text-indigo-600'
                                : 'border-transparent text-[hsl(var(--muted-foreground))] hover:text-foreground'
                                }`}
                        >
                            {tab.label}
                            {tab.count !== undefined && (
                                <span className="ml-2 px-2 py-0.5 text-xs rounded-full bg-[hsl(var(--accent))]">
                                    {tab.count}
                                </span>
                            )}
                        </button>
                    ))}
                </nav>
            </div>

            {/* Tab Content */}
            <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                {activeTab === 'overview' && (
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('fixedAssets.category')}</p>
                            <p className="font-medium">{asset.categoryName}</p>
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('fixedAssets.acquisitionDate')}</p>
                            <p className="font-medium">{new Date(asset.acquisitionDate).toLocaleDateString()}</p>
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('fixedAssets.acquisitionCost')}</p>
                            <p className="font-mono font-medium">{asset.acquisitionCost?.toLocaleString('en-US', { minimumFractionDigits: 2 })}</p>
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('fixedAssets.bookValue')}</p>
                            <p className="font-mono font-medium text-indigo-600">{asset.bookValue?.toLocaleString('en-US', { minimumFractionDigits: 2 })}</p>
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('fixedAssets.currentMeter')}</p>
                            <p className="font-mono font-medium">{asset.currentMeter?.toLocaleString('en-US')}</p>
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('fixedAssets.assignedTo')}</p>
                            <p className="font-medium">{asset.assignedEmployeeName || '—'}</p>
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('fixedAssets.serialNumber')}</p>
                            <p className="font-mono">{asset.serialNumber || '—'}</p>
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('fixedAssets.barCode')}</p>
                            <p className="font-mono">{asset.barCode || '—'}</p>
                        </div>
                    </div>
                )}

                {activeTab === 'assignments' && (
                    <div className="space-y-4">
                        {assignments.length === 0 ? (
                            <p className="text-center text-[hsl(var(--muted-foreground))] py-8">{t('common.noData')}</p>
                        ) : (
                            <table className="w-full">
                                <thead>
                                    <tr className="border-b">
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.employee')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.assignedDate')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.returnedDate')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.startMeter')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.endMeter')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.condition')}</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {assignments.map((a) => (
                                        <tr key={a.id} className="border-b last:border-0">
                                            <td className="py-3">{a.employeeName}</td>
                                            <td className="py-3">{new Date(a.assignedDate).toLocaleDateString()}</td>
                                            <td className="py-3">{a.returnedDate ? new Date(a.returnedDate).toLocaleDateString() : '—'}</td>
                                            <td className="py-3 font-mono">{a.startMeterValue.toLocaleString()}</td>
                                            <td className="py-3 font-mono">{a.endMeterValue?.toLocaleString() || '—'}</td>
                                            <td className="py-3">{a.endCondition || a.startCondition}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                    </div>
                )}

                {activeTab === 'meterLogs' && (
                    <div className="space-y-4">
                        {meterLogs.length === 0 ? (
                            <p className="text-center text-[hsl(var(--muted-foreground))] py-8">{t('common.noData')}</p>
                        ) : (
                            <table className="w-full">
                                <thead>
                                    <tr className="border-b">
                                        <th className="text-left py-2 font-medium">{t('common.date')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.meterValue')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.source')}</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {meterLogs.map((log) => (
                                        <tr key={log.id} className="border-b last:border-0">
                                            <td className="py-3">{new Date(log.logDate).toLocaleDateString()}</td>
                                            <td className="py-3 font-mono">{log.meterValue.toLocaleString()}</td>
                                            <td className="py-3">{t(`fixedAssets.meterSource.${log.source}`)}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                    </div>
                )}

                {activeTab === 'incidents' && (
                    <div className="space-y-4">
                        {incidents.length === 0 ? (
                            <p className="text-center text-[hsl(var(--muted-foreground))] py-8">{t('common.noData')}</p>
                        ) : (
                            <table className="w-full">
                                <thead>
                                    <tr className="border-b">
                                        <th className="text-left py-2 font-medium">{t('common.date')}</th>
                                        <th className="text-left py-2 font-medium">{t('common.description')}</th>
                                        <th className="text-left py-2 font-medium">{t('common.status')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.userFault')}</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {incidents.map((inc) => (
                                        <tr key={inc.id} className="border-b last:border-0">
                                            <td className="py-3">{new Date(inc.incidentDate).toLocaleDateString()}</td>
                                            <td className="py-3">{inc.description}</td>
                                            <td className="py-3">
                                                <Badge variant={inc.status === 3 ? 'success' : inc.status === 0 ? 'warning' : 'default'}>
                                                    {t(`fixedAssets.incidentStatus.${inc.status}`)}
                                                </Badge>
                                            </td>
                                            <td className="py-3">{inc.isUserFault === true ? t('common.yes') : inc.isUserFault === false ? t('common.no') : '—'}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                    </div>
                )}

                {activeTab === 'maintenance' && (
                    <div className="space-y-4">
                        {maintenances.length === 0 ? (
                            <p className="text-center text-[hsl(var(--muted-foreground))] py-8">{t('common.noData')}</p>
                        ) : (
                            <table className="w-full">
                                <thead>
                                    <tr className="border-b">
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.serviceDate')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.supplier')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.cost')}</th>
                                        <th className="text-left py-2 font-medium">{t('common.description')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.nextService')}</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {maintenances.map((m) => (
                                        <tr key={m.id} className="border-b last:border-0">
                                            <td className="py-3">{new Date(m.serviceDate).toLocaleDateString()}</td>
                                            <td className="py-3">{m.supplierName}</td>
                                            <td className="py-3 font-mono">{m.cost.toLocaleString('en-US', { minimumFractionDigits: 2 })}</td>
                                            <td className="py-3">{m.description}</td>
                                            <td className="py-3">{m.nextServiceDate ? new Date(m.nextServiceDate).toLocaleDateString() : '—'}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                    </div>
                )}

                {activeTab === 'disposals' && (
                    <div className="space-y-4">
                        {disposals.length === 0 ? (
                            <p className="text-center text-[hsl(var(--muted-foreground))] py-8">{t('common.noData')}</p>
                        ) : (
                            <table className="w-full">
                                <thead>
                                    <tr className="border-b">
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.disposalDate')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.type')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.saleAmount')}</th>
                                        <th className="text-left py-2 font-medium">{t('fixedAssets.profitLoss')}</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {disposals.map((d) => (
                                        <tr key={d.id} className="border-b last:border-0">
                                            <td className="py-3">{new Date(d.disposalDate).toLocaleDateString()}</td>
                                            <td className="py-3">{t(`fixedAssets.disposalType.${d.type}`)}</td>
                                            <td className="py-3 font-mono">{d.saleAmount?.toLocaleString('en-US', { minimumFractionDigits: 2 }) || '—'}</td>
                                            <td className="py-3 font-mono">{d.profitLoss.toLocaleString('en-US', { minimumFractionDigits: 2 })}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                    </div>
                )}
            </div>

            {/* Dialogs */}
            {asset && (
                <>
                    <AssetAssignDialog open={assignDialogOpen} onClose={handleDialogClose} assetId={asset.id} />
                    <AssetReturnDialog open={returnDialogOpen} onClose={handleDialogClose} assetId={asset.id} />
                    <AssetMeterLogDialog open={meterDialogOpen} onClose={handleDialogClose} assetId={asset.id} />
                    <AssetIncidentDialog open={incidentDialogOpen} onClose={handleDialogClose} assetId={asset.id} />
                    <AssetMaintenanceDialog open={maintenanceDialogOpen} onClose={handleDialogClose} assetId={asset.id} />
                    <AssetDisposeDialog open={disposeDialogOpen} onClose={handleDialogClose} assetId={asset.id} />
                </>
            )}
        </div>
    );
}
