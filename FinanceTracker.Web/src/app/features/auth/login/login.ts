import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { email } from '@angular/forms/signals';
import { ThemeToggle } from '../../../shared/components/theme-toggle/theme-toggle';
import {LucideAngularModule, Eye, EyeOff} from 'lucide-angular';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, ThemeToggle, LucideAngularModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  protected readonly isPasswordVisible = signal(false);
  protected readonly eyeIcon = Eye;
  protected readonly eyeOffIcon = EyeOff;

  private formBuilder = inject(FormBuilder);
  private authenticationService = inject(AuthService);
  private router = inject(Router);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal <string | null>(null);

  readonly loginForm = this.formBuilder.nonNullable.group({
    email:['',[Validators.required, Validators.email]],
    password:['',[Validators.required]],
  });

  submitLogin(): void {
    if(this.loginForm.invalid){
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.authenticationService.login(this.loginForm.getRawValue()).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: () => {
        this.errorMessage.set('Mail o contraseña no coincide.');
        this.isSubmitting.set(false);
      },
    });
  }
}
