import { useTranslation } from 'react-i18next';
import { Globe } from 'lucide-react';

export function LanguageSwitcher() {
    const { i18n } = useTranslation();

    const toggleLanguage = () => {
        const newLang = i18n.language === 'tr' ? 'en' : 'tr';
        i18n.changeLanguage(newLang);
        localStorage.setItem('language', newLang);
    };

    return (
        <button
            onClick={toggleLanguage}
            className="flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
            title={i18n.language === 'tr' ? 'Switch to English' : 'Türkçe\'ye geç'}
        >
            <Globe className="w-4 h-4" />
            <span className="text-sm font-medium uppercase">{i18n.language}</span>
        </button>
    );
}
