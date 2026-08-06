import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeToggle } from '../../../shared/components/theme-toggle/theme-toggle';
import { single } from 'rxjs';
import { LucideAngularModule, Eye, EyeOff } from 'lucide-angular';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, ThemeToggle, LucideAngularModule],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  readonly isPasswordVisible = signal(false);
  protected readonly eyeIcon = Eye;
  protected readonly eyeOffIcon = EyeOff;

  private formBuilder = inject(FormBuilder);
  private authenticationService = inject(AuthService);
  private router = inject(Router);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null> (null);
 
  readonly registerForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    email:['',[Validators.required, Validators.email]],
    password:['',[Validators.required, Validators.minLength(8)]],
  });

  submitRegister(): void {
    if (this.registerForm.invalid)
    {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.authenticationService.register(this.registerForm.getRawValue()).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (errorResponse) => {
        this.errorMessage.set(
          errorResponse.status === 409 ? 'Ya existe una cuenta con este mail.' : 'No pudimos crear la cuenta, intente de nuevo.'
        );
        this.isSubmitting.set(false);
      },
    });
  }
}
