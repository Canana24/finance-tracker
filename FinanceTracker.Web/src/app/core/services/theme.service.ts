import { Injectable, signal } from "@angular/core";

type ThemeName = 'dark' | 'light';

const THEME_STORAGE_KEY = 'ft_theme';

@Injectable( {providedIn: 'root'})
export class ThemeService {

    readonly currentTheme = signal<ThemeName>('dark');

    initializeTheme(): void {
        const storedTheme = localStorage.getItem(THEME_STORAGE_KEY) as ThemeName | null;
        const themeToApply: ThemeName = storedTheme ?? 'dark';
        this.applyTheme(themeToApply);
    }

    toggleTheme(): void {
        const nextTheme: ThemeName = this.currentTheme() === 'dark' ? 'light' : 'dark';
        this.applyTheme(nextTheme);
    }

    private applyTheme (theme: ThemeName): void{
        this.currentTheme.set(theme);
        localStorage.setItem(THEME_STORAGE_KEY, theme);

        const rootElement = document.documentElement;

        if(theme === 'light'){
            rootElement.setAttribute('data-theme','light');
        }
        else{
            rootElement.setAttribute('data-theme','dark');
        }
    }

}