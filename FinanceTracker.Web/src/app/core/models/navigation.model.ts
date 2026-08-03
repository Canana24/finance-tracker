export interface NavigationItem {
    label: string;
    route: string;
    icon: string;
}

export const navigationItems: NavigationItem [] =
[
    {label: 'Inicio', route: '/dashboard', icon: 'home'},
    {label: 'Movimientos', route: '/transactions', icon: 'arrow-left-right'},
    {label: 'Cuentas', route: '/accounts', icon: 'wallet'},
    {label: 'Categorías', route: '/categories', icon: 'tags'},
];