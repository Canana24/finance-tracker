import { Component, inject, signal, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { LucideAngularModule, Trash2 } from 'lucide-angular';
import { AccountService } from '../../core/services/account.service';
import { CurrencyService } from '../../core/services/currency.service';
import { Account } from '../../core/models/account.model';
import { Currency } from '../../core/models/currency.model';
import { Modal } from '../../shared/components/modal/modal';

@Component({
  selector: 'app-accounts',
  imports: [ReactiveFormsModule, Modal, LucideAngularModule, DecimalPipe],
  templateUrl: './accounts.html',
  styleUrl: './accounts.scss',
})
export class Accounts implements OnInit{
  private accountService = inject(AccountService);
  private currencyService = inject(CurrencyService);
  private formBuilder = inject(FormBuilder);

  protected readonly accounts = signal<Account[]>([]);
  protected readonly currencies = signal<Currency[]>([]);
  protected readonly isLoading = signal(true);

  protected readonly isModalOpen = signal(false);
  protected readonly editingAccountId = signal<number | null>(null);
  protected readonly deletingAccount = signal<Account | null>(null);
  protected readonly isSaving = signal(false);

  protected readonly trashIcon = Trash2;

  protected readonly accountForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    currencyId: [0, [Validators.required, Validators.min(1)]],
    initialBalance: [0, [Validators.required]],
  });

  ngOnInit(): void {
    this.loadAccounts();
    this.loadCurrencies();
  }

  private loadAccounts(): void {
    this.accountService.getAllAccounts().subscribe({
      next: (data) => {
        this.accounts.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  private loadCurrencies(): void {
    this.currencyService.getAllCurrencies().subscribe({
      next: (data) => {
        this.currencies.set(data);
      },
    });
  }

  openCreateModal(): void {
    this.editingAccountId.set(null);
    this.accountForm.reset({ name: '', currencyId: 0, initialBalance: 0});
    this.isModalOpen.set(true);
  }

  openEditModal(account: Account): void {
    const matchingCurrency = this.currencies().find(currency => currency.code === account.currencyCode);

    this.editingAccountId.set(account.id);
    this.accountForm.setValue({
      name: account.name,
      currencyId: matchingCurrency ? matchingCurrency.id : 0,
      initialBalance: 0,
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveAccount(): void {
    if(this.accountForm.invalid){
      this.accountForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    const formValue = this.accountForm.getRawValue();
    const editingId = this.editingAccountId();

    if(editingId === null){
      this.accountService.createAccount({
        name: formValue.name,
        currencyId: formValue.currencyId,
        initialBalance: formValue.initialBalance,
      }).subscribe({
        next: () => this.onSaveSuccess(),
        error: () => this.isSaving.set(false),
      });
    } else{
      this.accountService.updateAccount(
      editingId,
      {
        name: formValue.name,
        currencyId: formValue.currencyId,
      }).subscribe({
        next: () => this.onSaveSuccess(),
        error: () => this.isSaving.set(false),
      });
    }
  }

  askDeleteConfirmation(account: Account): void{
    this.deletingAccount.set(account);
  }

  cancelDelete(): void {
    this.deletingAccount.set(null);
  }

  confirmDeleteAccount(): void {
    const account = this.deletingAccount();
    if(account === null){
      return;
    }

    this.isSaving.set(true);
    this.accountService.deleteAccount(account.id).subscribe({
      next: () => {
        this.isSaving.set(true);
        this.deletingAccount.set(null);
        this.loadAccounts();
      },
      error: () => this.isSaving.set(false),
    });
  }

  private onSaveSuccess(): void {
    this.isSaving.set(false);
    this.closeModal();
    this.loadAccounts();
  }
}
