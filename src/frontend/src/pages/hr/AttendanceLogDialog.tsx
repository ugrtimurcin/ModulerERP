import { useState, useEffect, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, useToast } from '@/components/ui';
import { X, MapPin, RefreshCw, Upload } from 'lucide-react';
import { Html5Qrcode } from 'html5-qrcode';
import { api } from '@/lib/api';

interface AttendanceLogDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
}

export function AttendanceLogDialog({ open, onClose }: AttendanceLogDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isLoading, setIsLoading] = useState(false);
    const [employees, setEmployees] = useState<{ id: string, firstName: string, lastName: string }[]>([]);
    const [uploadedImage, setUploadedImage] = useState<string | null>(null);

    const scannerRef = useRef<Html5Qrcode | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);
    // Ref to track if we are currently ensuring scanner
    const initializingRef = useRef(false);
    const isScanningRef = useRef(false);

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
            //toast.success(t('hr.scanSuccess'));
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
        setUploadedImage(null);

        try {
            // Cleanup existing first
            if (scannerRef.current) {
                try {
                    await scannerRef.current.stop();
                    await scannerRef.current.clear();
                } catch (e) {
                    // console.warn("Failed to stop/clear scanner", e);
                }
                scannerRef.current = null;
            }

            // Small delay to ensure DOM and cleanup
            await new Promise(r => setTimeout(r, 100));

            const element = document.getElementById('camera-reader');
            if (element && open) {
                const html5QrCode = new Html5Qrcode("camera-reader");
                scannerRef.current = html5QrCode;

                try {
                    await html5QrCode.start(
                        { facingMode: "environment" },
                        {
                            fps: 10,
                            qrbox: { width: 250, height: 250 }
                        },
                        (decodedText) => {
                            handleScanSuccess(decodedText);
                            // html5QrCode.pause(); // Do not pause, keep scanning or stop? 
                            // Pausing might cause "not running" error if we try to stop later?
                            // Let's keep it running until close or upload.
                        },
                        () => {
                            // error / invalid scan, ignore
                        }
                    );
                    isScanningRef.current = true;
                } catch (err) {
                    isScanningRef.current = false;
                    console.error("Error starting scanner", err);
                    // Show manual entry or error message
                }
            }
        } catch (err) {
            // Scanner init error
        } finally {
            initializingRef.current = false;
        }
    }, [open, handleScanSuccess]);

    const stopScanner = useCallback(() => {
        if (scannerRef.current) {
            // Only attempt to stop if we believe it's running
            if (isScanningRef.current) {
                scannerRef.current.stop().then(() => {
                    scannerRef.current?.clear();
                    scannerRef.current = null;
                    isScanningRef.current = false;
                }).catch(err => {
                    console.warn("Scanner stop failed", err);
                    scannerRef.current = null;
                    isScanningRef.current = false;
                });
            } else {
                // Just clear reference if not running
                scannerRef.current = null;
            }
        }
    }, []);

    const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        // Create preview
        const imageUrl = URL.createObjectURL(file);
        setUploadedImage(imageUrl);

        // Stop current scanner to free up the UI element
        if (scannerRef.current) {
            try {
                if (isScanningRef.current) {
                    await scannerRef.current.stop();
                    isScanningRef.current = false;
                }
                await scannerRef.current.clear();
                scannerRef.current = null;
            } catch (err) {
                console.warn("Failed to stop scanner for file upload", err);
            }
        }

        // Wait a bit for cleanup
        setTimeout(async () => {
            try {
                const html5QrCode = new Html5Qrcode("camera-reader");
                scannerRef.current = html5QrCode;
                const decodedText = await html5QrCode.scanFile(file, false);
                handleScanSuccess(decodedText);
            } catch (err) {
                console.error("File scan failed", err);
                //toast.error(t('common.error'), t('hr.scanFailed'));
                setShowErrorTick(true);
            }
            // Reset input
            if (fileInputRef.current) fileInputRef.current.value = '';
        }, 100);
    };

    useEffect(() => {
        if (open) {
            api.get<{ id: string, firstName: string, lastName: string }[]>('/hr/employees')
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
        setUploadedImage(null);
    };

    const handleTryAgain = () => {
        setShowErrorTick(false);
        setFormData(prev => ({ ...prev, employeeId: '' }));
        setUploadedImage(null);
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
            () => {
                setLocationStatus('error');
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            }
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

            await api.post('/hr/attendance/scan', payload);
            toast.success(t('hr.scanLogged'));
            onClose(true);
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
                    <div className="relative w-full overflow-hidden rounded-lg bg-gray-100 min-h-[250px] flex items-center justify-center bg-black">
                        <div id="camera-reader" className={`w-full ${uploadedImage ? 'hidden' : ''}`}></div>

                        {uploadedImage && (
                            <img src={uploadedImage} alt="Scanning..." className="max-w-full max-h-[300px] object-contain" />
                        )}

                        {/* File Upload Trigger */}
                        {!uploadedImage && (
                            <div className="absolute top-2 right-2 z-20">
                                <input
                                    type="file"
                                    ref={fileInputRef}
                                    className="hidden"
                                    accept="image/*"
                                    onChange={handleImageUpload}
                                />
                                <Button
                                    type="button"
                                    variant="secondary"
                                    size="sm"
                                    className="bg-white/80 backdrop-blur"
                                    onClick={() => fileInputRef.current?.click()}
                                    title={t('hr.uploadQr')}
                                >
                                    <Upload className="w-4 h-4" />
                                </Button>
                            </div>
                        )}

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

                    <LocationStatus status={locationStatus} coordinates={formData.gpsCoordinates} t={t} />
                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>

                        {isError ? (
                            <Button onClick={handleTryAgain} variant="secondary" className="flex-1">
                                <RefreshCw className="w-4 h-4 mr-2" />
                                {/* ... */}
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



function LocationStatus({ status, t }: any) {
    return (
        <div className="bg-[hsl(var(--secondary))] rounded-lg p-3 flex items-center justify-between text-sm">
            <div className="flex items-center gap-2">
                <MapPin className="w-4 h-4 text-gray-500" />
                <span>{t('hr.location')}</span>
            </div>
            <div className="text-right">
                <span className={`block ${status === 'found' ? 'text-emerald-600 font-medium' : 'text-gray-500'}`}>
                    {status === 'locating' && t('common.loading')}
                    {status === 'found' && t('hr.acquired')}
                    {status === 'error' && t('hr.notAvailable')}
                    {status === 'idle' && '-'}
                </span>
            </div>
        </div>
    );
}
