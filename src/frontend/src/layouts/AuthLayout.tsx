import { Outlet, Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/stores';

export function AuthLayout() {
    const { t } = useTranslation();
    const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

    if (isAuthenticated) {
        return <Navigate to="/dashboard" replace />;
    }

    return (
        <div className="min-h-screen flex">
            {/* Left side - Form */}
            <div className="flex-1 flex items-center justify-center p-8">
                <div className="w-full max-w-md">
                    <Outlet />
                </div>
            </div>

            {/* Right side - Branding */}
            <div className="hidden lg:flex lg:flex-1 bg-gradient-to-br from-indigo-600 to-purple-700 items-center justify-center p-12">
                <div className="text-white text-center">
                    <h1 className="text-4xl font-bold mb-4">{t('auth.brandTitle')}</h1>
                    <p className="text-lg opacity-90 max-w-md">
                        {t('auth.brandSubtitle')}
                    </p>
                    <div className="mt-12 grid grid-cols-2 gap-6 text-left">
                        <div className="bg-white/10 p-4 rounded-lg backdrop-blur-sm">
                            <h3 className="font-semibold mb-1">{t('auth.features.modules.title')}</h3>
                            <p className="text-sm opacity-75">{t('auth.features.modules.desc')}</p>
                        </div>
                        <div className="bg-white/10 p-4 rounded-lg backdrop-blur-sm">
                            <h3 className="font-semibold mb-1">{t('auth.features.entities.title')}</h3>
                            <p className="text-sm opacity-75">{t('auth.features.entities.desc')}</p>
                        </div>
                        <div className="bg-white/10 p-4 rounded-lg backdrop-blur-sm">
                            <h3 className="font-semibold mb-1">{t('auth.features.multiCurrency.title')}</h3>
                            <p className="text-sm opacity-75">{t('auth.features.multiCurrency.desc')}</p>
                        </div>
                        <div className="bg-white/10 p-4 rounded-lg backdrop-blur-sm">
                            <h3 className="font-semibold mb-1">{t('auth.features.multiTenant.title')}</h3>
                            <p className="text-sm opacity-75">{t('auth.features.multiTenant.desc')}</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
