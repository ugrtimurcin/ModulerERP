import { useState, useEffect, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, useToast } from '@/components/ui';
import { X, MapPin, RefreshCw } from 'lucide-react';
import { Html5QrcodeScanner } from 'html5-qrcode';

interface AttendanceLogDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
}

export function AttendanceLogDialog({ open, onClose }: AttendanceLogDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isLoading, setIsLoading] = useState(false);
    const [employees, setEmployees] = useState<{ id: string, firstName: string, lastName: string }[]>([]);

    const scannerRef = useRef<Html5QrcodeScanner | null>(null);
    // Ref to track if we are currently ensuring scanner
    const initializingRef = useRef(false);

    const [locationStatus, setLocationStatus] = useState<'idle' | 'locating' | 'found' | 'error'>('idle');
    const [showSuccessTick, setShowSuccessTick] = useState(false);
    const [showErrorTick, setShowErrorTick] = useState(false);

    const [formData, setFormData] = useState({
        employeeId: '',
        type: 1,
        transactionTime: new Date().toISOString().slice(0, 16),
        supervisorId: '',
        gpsCoordinates: ''
    });

    // Ref to access latest employees inside closure
    const employeesRef = useRef<{ id: string, firstName: string, lastName: string }[]>([]);
    useEffect(() => {
        employeesRef.current = employees;
    }, [employees]);

    // Validation Effect - Triggered when employeeId changes
    useEffect(() => {
        if (!formData.employeeId) return;

        const exists = employeesRef.current.some(e => e.id === formData.employeeId);
        if (exists) {
            setShowSuccessTick(true);
            toast.success(t('hr.scanSuccess'));
        } else {
            setShowErrorTick(true);
        }
    }, [formData.employeeId, t]);

    const handleScanSuccess = useCallback((decodedText: string) => {
        setFormData(prev => ({ ...prev, employeeId: decodedText }));
        // Validation effect acts on this state change
    }, []);

    const startScanner = useCallback(async () => {
        if (initializingRef.current) return;
        initializingRef.current = true;

        try {
            // Cleanup existing first
            if (scannerRef.current) {
                try {
                    await scannerRef.current.clear();
                } catch (e) {
                    console.warn("Failed to clear scanner", e);
                }
                scannerRef.current = null;
            }

            // Small delay to ensure DOM and cleanup
            await new Promise(r => setTimeout(r, 100));

            const element = document.getElementById('camera-reader');
            if (element && open) {
                const scanner = new Html5QrcodeScanner(
                    "camera-reader",
                    { fps: 10, qrbox: { width: 250, height: 250 } },
                    false
                );
                scannerRef.current = scanner;

                // Render can throw if element issue
                scanner.render((decodedText) => {
                    handleScanSuccess(decodedText);
                    // Stop scanning on success? 
                    // Usually we want to pause or stop. 
                    // If we clear, the camera goes away.
                    // User wants result shown in green box.
                    // We will clear it to show the Tick animation clearly?
                    // "animated tick should be placed in camera area, but if employee does not exist ... try again button. when user click it camera should be appear"
                    // This implies camera DISAPPEARS or gets covered.
                    // Our overlay covers it, so we can keep camera running behind or stop it.
                    // Stopping saves battery. Let's stop it.
                    if (scannerRef.current) {
                        scannerRef.current.clear().catch(console.error);
                        scannerRef.current = null;
                    }
                }, () => {
                    // Suppress scanning errors
                });
            }
        } catch (err) {
            // Scanner init error
        } finally {
            initializingRef.current = false;
        }
    }, [open, handleScanSuccess]);

    const stopScanner = useCallback(() => {
        if (scannerRef.current) {
            scannerRef.current.clear().catch(console.error);
            scannerRef.current = null;
        }
    }, []);

    useEffect(() => {
        if (open) {
            fetch('/api/hr/employees')
                .then(res => res.json())
                .then(data => setEmployees(data))
                .catch(err => console.error("Failed to load employees", err));

            resetState();
            getLocation();

            // Timeout to allow modal to render
            const timer = setTimeout(() => {
                startScanner();
            }, 300);
            return () => clearTimeout(timer);
        } else {
            stopScanner();
        }
    }, [open, startScanner, stopScanner]);

    const resetState = () => {
        setFormData(prev => ({
            ...prev,
            employeeId: '',
            transactionTime: new Date().toISOString().slice(0, 16),
            gpsCoordinates: ''
        }));
        setLocationStatus('idle');
        setShowSuccessTick(false);
        setShowErrorTick(false);
    };

    const handleTryAgain = () => {
        setShowErrorTick(false);
        setFormData(prev => ({ ...prev, employeeId: '' }));
        // Re-init scanner
        startScanner();
    };

    const getLocation = () => {
        if (!navigator.geolocation) {
            setLocationStatus('error');
            return;
        }
        setLocationStatus('locating');
        navigator.geolocation.getCurrentPosition(
            (position) => {
                const coords = `${position.coords.latitude},${position.coords.longitude}`;
                setFormData(prev => ({ ...prev, gpsCoordinates: coords }));
                setLocationStatus('found');
            },
            () => setLocationStatus('error')
        );
    };

    const handleScanSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        submitLog();
    };

    const submitLog = async () => {
        setIsLoading(true);
        setShowSuccessTick(false);
        setShowErrorTick(false);
        try {
            // Prioritize captured GPS
            const finalGps = formData.gpsCoordinates || 'GPS Error/Not Permitted';

            const payload: any = {
                ...formData,
                gpsCoordinates: finalGps,
                transactionTime: new Date(formData.transactionTime).toISOString()
            };
            if (!payload.supervisorId) payload.supervisorId = '30cc1aa6-fc36-455c-bfd3-784c6732298e';

            const res = await fetch('/api/hr/attendance-logs', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload),
            });

            if (res.ok) {
                toast.success(t('hr.scanLogged'));
                onClose(true);
            } else {
                setShowErrorTick(true);
                try {
                    const errData = await res.json();
                    if (errData.errors) {
                    } else {
                    }
                } catch {
                }
            }
        } catch {
            setShowErrorTick(true);
        }
        setIsLoading(false);
    };

    if (!open) return null;

    const scannedEmployee = employees.find(e => e.id === formData.employeeId);
    // Is valid if we have an ID and we found the employee
    const isValidScan = !!scannedEmployee;
    // Is error state active?
    const isError = showErrorTick || (formData.employeeId && !isValidScan);

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-lg border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{t('hr.logAttendance')}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <div className="p-6 space-y-6">
                    <div className="relative w-full overflow-hidden rounded-lg bg-gray-100 min-h-[250px]">
                        <div id="camera-reader" className="w-full"></div>

                        {(showSuccessTick || showErrorTick) && (
                            <style>{`
                                .checkmark__circle {
                                    stroke-dasharray: 166;
                                    stroke-dashoffset: 166;
                                    stroke-width: 2;
                                    stroke-miterlimit: 10;
                                    stroke: #22c55e;
                                    fill: none;
                                    animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
                                }
                                .checkmark {
                                    width: 80px;
                                    height: 80px;
                                    border-radius: 50%;
                                    display: block;
                                    stroke-width: 2;
                                    stroke: #fff;
                                    stroke-miterlimit: 10;
                                    box-shadow: inset 0px 0px 0px #22c55e;
                                    animation: fill .4s ease-in-out .4s forwards, scale .3s ease-in-out .9s both;
                                }
                                .checkmark__check {
                                    transform-origin: 50% 50%;
                                    stroke-dasharray: 48;
                                    stroke-dashoffset: 48;
                                    animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.8s forwards;
                                }
                                
                                .checkmark__circle_error {
                                    stroke-dasharray: 166;
                                    stroke-dashoffset: 166;
                                    stroke-width: 2;
                                    stroke-miterlimit: 10;
                                    stroke: #ef4444;
                                    fill: none;
                                    animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
                                }
                                .checkmark_error {
                                    width: 80px;
                                    height: 80px;
                                    border-radius: 50%;
                                    display: block;
                                    stroke-width: 2;
                                    stroke: #fff;
                                    stroke-miterlimit: 10;
                                    box-shadow: inset 0px 0px 0px #ef4444;
                                    animation: fill_error .4s ease-in-out .4s forwards, scale .3s ease-in-out .9s both;
                                }
                                .checkmark__check_error {
                                    transform-origin: 50% 50%;
                                    stroke-dasharray: 48;
                                    stroke-dashoffset: 48;
                                    animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.8s forwards;
                                }

                                @keyframes stroke { 100% { stroke-dashoffset: 0; } }
                                @keyframes fill { 100% { box-shadow: inset 0px 0px 0px 40px #22c55e; } }
                                @keyframes fill_error { 100% { box-shadow: inset 0px 0px 0px 40px #ef4444; } }
                                @keyframes scale { 0%, 100% { transform: none; } 50% { transform: scale3d(1.1, 1.1, 1); } }
                            `}</style>
                        )}

                        {showSuccessTick && (
                            <div className="absolute inset-0 z-10 flex items-center justify-center bg-white/90 backdrop-blur-sm animate-in fade-in duration-300">
                                <svg className="checkmark" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 52 52">
                                    <circle className="checkmark__circle" cx="26" cy="26" r="25" fill="none" />
                                    <path className="checkmark__check" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8" />
                                </svg>
                            </div>
                        )}

                        {showErrorTick && (
                            <div className="absolute inset-0 z-10 flex flex-col items-center justify-center bg-white/90 backdrop-blur-sm animate-in fade-in duration-300 gap-4">
                                <svg className="checkmark_error" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 52 52">
                                    <circle className="checkmark__circle_error" cx="26" cy="26" r="25" fill="none" />
                                    <path className="checkmark__check_error" fill="none" d="M16 16 36 36 M36 16 16 36" />
                                </svg>
                            </div>
                        )}
                    </div>

                    {formData.employeeId && (
                        <div className={`text-center p-3 rounded-lg transition-colors duration-300 ${isValidScan ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'
                            }`}>
                            {isValidScan ? (
                                <span className="font-semibold flex items-center justify-center gap-2">
                                    Scanned: {scannedEmployee?.firstName} {scannedEmployee?.lastName}
                                </span>
                            ) : (
                                <span className="font-semibold flex items-center justify-center gap-2">
                                    {t('common.anErrorOccurred')}
                                </span>
                            )}
                        </div>
                    )}

                    <TypeSelector type={formData.type} onChange={(val) => setFormData(prev => ({ ...prev, type: val }))} t={t} />
                    <LocationStatus status={locationStatus} t={t} />
                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>

                        {isError ? (
                            <Button onClick={handleTryAgain} variant="secondary" className="flex-1">
                                <RefreshCw className="w-4 h-4 mr-2" />
                                {t('common.tryAgain')}
                            </Button>
                        ) : (
                            <Button onClick={handleScanSubmit} disabled={isLoading || !isValidScan}>
                                {t('hr.logScan')}
                            </Button>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}

function TypeSelector({ type, onChange, t }: { type: number, onChange: (v: number) => void, t: any }) {
    return (
        <div>
            <label className="block text-sm font-medium mb-1.5">{t('common.type')}</label>
            <div className="grid grid-cols-2 gap-3">
                <button
                    type="button"
                    onClick={() => onChange(1)}
                    className={`px-3 py-2 rounded-lg border text-sm font-medium transition-all ${type === 1
                        ? 'border-emerald-500 bg-emerald-50 text-emerald-700 dark:bg-emerald-900/20 dark:text-emerald-400'
                        : 'border-[hsl(var(--border))] hover:bg-[hsl(var(--accent))]'}`}
                >
                    {t('hr.checkIn')}
                </button>
                <button
                    type="button"
                    onClick={() => onChange(2)}
                    className={`px-3 py-2 rounded-lg border text-sm font-medium transition-all ${type === 2
                        ? 'border-amber-500 bg-amber-50 text-amber-700 dark:bg-amber-900/20 dark:text-amber-400'
                        : 'border-[hsl(var(--border))] hover:bg-[hsl(var(--accent))]'}`}
                >
                    {t('hr.checkOut')}
                </button>
            </div>
        </div>
    );
}

function LocationStatus({ status, t }: any) {
    return (
        <div className="bg-[hsl(var(--secondary))] rounded-lg p-3 flex items-center justify-between text-sm">
            <div className="flex items-center gap-2">
                <MapPin className="w-4 h-4 text-gray-500" />
                <span>{t('hr.location')}</span>
            </div>
            <span className={status === 'found' ? 'text-emerald-600 font-medium' : 'text-gray-500'}>
                {status === 'locating' && t('common.loading')}
                {status === 'found' && t('hr.acquired')}
                {status === 'error' && t('hr.notAvailable')}
                {status === 'idle' && '-'}
            </span>
        </div>
    );
}
