import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Loader2 } from 'lucide-react';
import { useAuthStore } from '@/stores';
import { api } from '@/services/api';
import { LanguageSwitcher } from '@/components/LanguageSwitcher';

export function LoginPage() {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const login = useAuthStore((state) => state.login);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        setError(null);

        try {
            const result = await api.auth.login(email, password);

            if (result.success && result.data) {
                login(
                    {
                        id: result.data.user.id,
                        email: result.data.user.email,
                        firstName: result.data.user.firstName,
                        lastName: result.data.user.lastName,
                        tenantId: result.data.user.tenantId,
                        roles: result.data.user.roles,
                    },
                    result.data.accessToken,
                    result.data.refreshToken
                );
                navigate('/dashboard');
            } else {
                setError(result.error || t('auth.invalidCredentials'));
            }
        } catch (err) {
            setError(t('auth.invalidCredentials'));
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div>
            <div className="flex justify-end mb-4">
                <LanguageSwitcher />
            </div>

            <h1 className="text-2xl font-bold mb-2">{t('auth.signIn')}</h1>
            <p className="text-[hsl(var(--muted-foreground))] mb-8">
                {t('auth.enterCredentials')}
            </p>

            {error && (
                <div className="mb-4 p-3 bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400 rounded-lg text-sm">
                    {error}
                </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium mb-1">{t('auth.email')}</label>
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        className="w-full px-3 py-2 border rounded-lg bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        placeholder="admin@erp.local"
                        required
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">{t('auth.password')}</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="w-full px-3 py-2 border rounded-lg bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        placeholder="••••••••"
                        required
                    />
                </div>

                <button
                    type="submit"
                    disabled={isLoading}
                    className="w-full py-2 px-4 bg-gradient-to-r from-indigo-600 to-purple-600 text-white rounded-lg font-medium hover:from-indigo-700 hover:to-purple-700 disabled:opacity-50 flex items-center justify-center transition-all"
                >
                    {isLoading ? (
                        <>
                            <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                            {t('common.loading')}
                        </>
                    ) : (
                        t('auth.signIn')
                    )}
                </button>
            </form>

            <p className="mt-6 text-center text-sm text-[hsl(var(--muted-foreground))]">
                {t('auth.demo')}
            </p>
        </div>
    );
}
