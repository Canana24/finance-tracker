import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { Register } from './register';
import { AuthService } from '../../../core/services/auth.service';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;
  let authService: { register: ReturnType<typeof vi.fn> };
  let router: Router;
  let navigateSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(async () => {
    authService = { register: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('does not call AuthService.register when the form is invalid', () => {
    component.submitRegister();

    expect(authService.register).not.toHaveBeenCalled();
    expect(component.registerForm.controls.name.touched).toBe(true);
  });

  it('rejects a password shorter than 8 characters', () => {
    component.registerForm.setValue({ name: 'Franco', email: 'franco@test.com', password: 'short' });

    expect(component.registerForm.invalid).toBe(true);
    expect(component.registerForm.controls.password.errors).toHaveProperty('minlength');
  });

  it('calls AuthService.register and navigates to /dashboard on success', () => {
    authService.register.mockReturnValue(of({ token: 'a-token', expiresAt: '2026-01-01T00:00:00Z' }));
    component.registerForm.setValue({ name: 'Franco', email: 'franco@test.com', password: 'Password123' });

    component.submitRegister();

    expect(authService.register).toHaveBeenCalledWith({
      name: 'Franco',
      email: 'franco@test.com',
      password: 'Password123',
    });
    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  });

  it('shows a duplicate-email message on a 409 response', () => {
    authService.register.mockReturnValue(throwError(() => ({ status: 409 })));
    component.registerForm.setValue({ name: 'Franco', email: 'ya-existe@test.com', password: 'Password123' });

    component.submitRegister();

    expect(component.errorMessage()).toBe('Ya existe una cuenta con este mail.');
    expect(component.isSubmitting()).toBe(false);
  });

  it('shows a generic message on a non-409 error', () => {
    authService.register.mockReturnValue(throwError(() => ({ status: 500 })));
    component.registerForm.setValue({ name: 'Franco', email: 'franco@test.com', password: 'Password123' });

    component.submitRegister();

    expect(component.errorMessage()).toBe('No pudimos crear la cuenta, intente de nuevo.');
  });
});
