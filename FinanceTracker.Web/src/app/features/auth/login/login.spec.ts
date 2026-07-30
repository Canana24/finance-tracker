import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { Login } from './login';
import { AuthService } from '../../../core/services/auth.service';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let authService: { login: ReturnType<typeof vi.fn> };
  let router: Router;
  let navigateSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(async () => {
    authService = { login: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('does not call AuthService.login when the form is invalid', () => {
    component.submitLogin();

    expect(authService.login).not.toHaveBeenCalled();
    expect(component.loginForm.controls.email.touched).toBe(true);
    expect(component.loginForm.controls.password.touched).toBe(true);
  });

  it('calls AuthService.login with the form values and navigates to /dashboard on success', () => {
    authService.login.mockReturnValue(of({ token: 'a-token', expiresAt: '2026-01-01T00:00:00Z' }));
    component.loginForm.setValue({ email: 'user@test.com', password: 'secret123' });

    component.submitLogin();

    expect(authService.login).toHaveBeenCalledWith({ email: 'user@test.com', password: 'secret123' });
    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
    // En éxito el componente no resetea isSubmitting explícitamente: confía en la
    // navegación para sacar al usuario de la pantalla.
    expect(component.isSubmitting()).toBe(true);
  });

  it('shows a generic error message and stops submitting when login fails', () => {
    authService.login.mockReturnValue(throwError(() => ({ status: 401 })));
    component.loginForm.setValue({ email: 'user@test.com', password: 'wrong-pass' });

    component.submitLogin();

    expect(component.errorMessage()).toBe('Mail o contraseña no coincide.');
    expect(component.isSubmitting()).toBe(false);
    expect(navigateSpy).not.toHaveBeenCalled();
  });
});
