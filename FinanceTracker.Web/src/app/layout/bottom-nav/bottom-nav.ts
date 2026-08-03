import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, LucideIconData, House, ArrowLeftRight, Wallet, Tags } from 'lucide-angular';
import { navigationItems } from '../../core/models/navigation.model';

@Component({
  selector: 'app-bottom-nav',
  imports: [RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './bottom-nav.html',
  styleUrl: './bottom-nav.scss',
})
export class BottomNav {
  protected readonly navigationItems = navigationItems;
  
  protected readonly iconMap: Record<string, LucideIconData> = {
    'home': House,
    'arrow-left-right': ArrowLeftRight,
    'wallet': Wallet,
    'tags': Tags,
  };
}
