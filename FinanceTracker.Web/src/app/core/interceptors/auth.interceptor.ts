import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (request, nextHandler) =>
{
    const authenticationService = inject(AuthService);
    const token = authenticationService.getToken();

    //Hay token?
    const requestWithToken = token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}`}})
    : request;

    return nextHandler (requestWithToken).pipe(
        catchError((error: HttpErrorResponse) => {
            if(error.status === 401){
                authenticationService.logout();
            }
            return throwError(() => error);
        })
    );
}