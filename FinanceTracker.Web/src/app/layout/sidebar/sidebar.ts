import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, LucideIconData, ArrowLeftRight, Wallet, Tags, LogOut, House } from 'lucide-angular';
import { navigationItems } from '../../core/models/navigation.model';
import { AuthService } from '../../core/services/auth.service';
import { ThemeToggle } from '../../shared/components/theme-toggle/theme-toggle';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, LucideAngularModule, ThemeToggle],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class Sidebar {

  private authenticationService = inject(AuthService);
  protected readonly navigationItems = navigationItems;

  protected readonly iconMap: Record<string, LucideIconData> ={
    'home': House,
    'arrow-left-right': ArrowLeftRight,
    'wallet': Wallet,
    'tags': Tags,
  };

  protected readonly logoutIcon = LogOut;

  logout(): void {
    this.authenticationService.logout(); 
  }

}
