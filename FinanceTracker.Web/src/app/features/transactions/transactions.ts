import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { LucideAngularModule, Trash2 } from 'lucide-angular';
import { TransactionService } from '../../core/services/transaction.service';
import { AccountService } from '../../core/services/account.service';
import { CategoryService } from '../../core/services/category.service';
import { Transaction } from '../../core/models/transaction.model';
import { Account } from '../../core/models/account.model';
import { Category, CategoryType } from '../../core/models/category.model';
import { Modal } from '../../shared/components/modal/modal';

@Component({
  selector: 'app-transactions',
  imports: [ReactiveFormsModule, Modal, LucideAngularModule, DecimalPipe, DatePipe],
  templateUrl: './transactions.html',
  styleUrl: './transactions.scss',
})
export class Transactions implements OnInit {
  private transactionService = inject(TransactionService);
  private accountService = inject(AccountService);
  private categoryService = inject(CategoryService);
  private formBuilder = inject(FormBuilder);

  protected readonly transactions = signal<Transaction[]>([]);
  protected readonly accounts = signal<Account[]>([]);
  protected readonly categories = signal<Category[]>([]);
  protected readonly isLoading = signal(true);

  protected readonly isModalOpen = signal(false);
  protected readonly editingTransactionId = signal<number | null>(null);
  protected readonly deletingTransaction = signal<Transaction | null>(null);
  protected readonly isSaving = signal(false);

  protected readonly selectedTransactionType = signal<CategoryType>('EXPENSE');

  protected readonly availableCategories = computed(() => this.categories().filter( category => category.type === this.selectedTransactionType()));

  protected readonly trashIcon = Trash2;

  protected readonly transactionForm = this.formBuilder.nonNullable.group(
    {
      accountId: [0, [Validators.required, Validators.min(1)]],
      categoryId: [0, [Validators.required, Validators.min(1)]],
      amount: [0, [Validators.required, Validators.min(0.01)]],
      type: ['EXPENSE' as CategoryType, [Validators.required]],
      description: [''],
      date: [this.formatDateForInput(new Date()), [Validators.required]], 
    }
  );



  ngOnInit(): void {
    this.loadTransactions();
    this.loadAccounts();
    this.loadCategories();
  }

  private loadTransactions(): void {
    this.transactionService.getAllTransactions().subscribe({
      next: (data) => {
        this.transactions.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  private loadAccounts(): void {
    this.accountService.getAllAccounts().subscribe({
      next: (data) => {
        this.accounts.set(data);
      },
    });
  }

  private loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (data) => {
        this.categories.set(data);
      },
    });
  }

  onTransactionTypeChange(): void {
    this.selectedTransactionType.set(this.transactionForm.controls.type.value);
    this.transactionForm.controls.categoryId.setValue(0);
  }

  openCreateModal(): void {
    this.editingTransactionId.set(null);
    this.selectedTransactionType.set('EXPENSE');
    this.transactionForm.reset(
      {
        accountId: 0,
        categoryId: 0,
        amount: 0,
        type: 'EXPENSE',
        description: '',
        date: this.formatDateForInput(new Date()),
      }
    );
    this.isModalOpen.set(true);
  }

  openEditModal(transaction: Transaction): void {
    this.editingTransactionId.set(transaction.id);
    this.selectedTransactionType.set(transaction.type);
    this.transactionForm.setValue({
      accountId: transaction.accountId,
      categoryId: transaction.categoryId,
      amount: transaction.amount,
      type: transaction.type,
      description: transaction.description ?? '',
      date: transaction.date.slice(0,10),
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveTransaction(): void{
    if (this.transactionForm.invalid){
      this.transactionForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    const formValue = this.transactionForm.getRawValue();
    const editingId = this.editingTransactionId();
    const descriptionToSend = formValue.description.trim() === '' ? null : formValue.description.trim();

    if(editingId === null) {
      this.transactionService.createTransaction({
        accountId: formValue.accountId,
        categoryId: formValue.categoryId,
        amount: formValue.amount,
        type: formValue.type,
        description: descriptionToSend,
        date: formValue.date,
      }).subscribe({
        next: () => this.onSaveSuccess(),
        error: () => this.isSaving.set(false),  
      });
    } else {
      this.transactionService.updateTransaction(editingId, 
      {
        categoryId: formValue.categoryId,
        amount: formValue.amount,
        description: descriptionToSend,
        date: formValue.date,
      }).subscribe({
        next: () => this.onSaveSuccess(),
        error: () => this.isSaving.set(false),
      });
    }
  }

  askDeleteConfirmation(transaction: Transaction): void {
    this.deletingTransaction.set(null);
  }

  cancelDelete(): void{
    this.deletingTransaction.set(null);
  }

  confirmDelete(): void {
    const transaction = this.deletingTransaction();
    if(transaction === null){
      return;
    }

    this.isSaving.set(true);
    this.transactionService.deleteTransaction(transaction.id).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.deletingTransaction.set(null);
        this.loadTransactions();
      },
      error: () => this.isSaving.set(false),
    });
  }

  private onSaveSuccess(): void {
    this.isSaving.set(false);
    this.closeModal();
    this.loadTransactions();
  }

  private formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
