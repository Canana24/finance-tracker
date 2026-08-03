import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authenticationGuard: CanActivateFn = () => {
    const authenticationService = inject(AuthService);
    const router = inject(Router);

    if(authenticationService.isLoggedIn()) {
        return true;
    }

    router.navigate(['/login']);
    return false;
}