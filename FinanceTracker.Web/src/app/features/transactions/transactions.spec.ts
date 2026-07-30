import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Transactions } from './transactions';
import { TransactionService } from '../../core/services/transaction.service';
import { AccountService } from '../../core/services/account.service';
import { CategoryService } from '../../core/services/category.service';
import { Category } from '../../core/models/category.model';
import { Transaction } from '../../core/models/transaction.model';

describe('Transactions', () => {
  let component: Transactions;
  let fixture: ComponentFixture<Transactions>;
  let transactionService: {
    getAllTransactions: ReturnType<typeof vi.fn>;
    createTransaction: ReturnType<typeof vi.fn>;
    updateTransaction: ReturnType<typeof vi.fn>;
    deleteTransaction: ReturnType<typeof vi.fn>;
  };
  let accountService: { getAllAccounts: ReturnType<typeof vi.fn> };
  let categoryService: { getAllCategories: ReturnType<typeof vi.fn> };

  const categories: Category[] = [
    { id: 1, name: 'Sueldo', type: 'INCOME', icon: null, createdAt: '2026-01-01' },
    { id: 2, name: 'Freelance', type: 'INCOME', icon: null, createdAt: '2026-01-01' },
    { id: 3, name: 'Comida', type: 'EXPENSE', icon: null, createdAt: '2026-01-01' },
  ];

  const sampleTransaction: Transaction = {
    id: 10,
    accountId: 1,
    accountName: 'Cuenta',
    categoryId: 3,
    categoryName: 'Comida',
    amount: 500,
    type: 'EXPENSE',
    description: 'Almuerzo',
    date: '2026-03-15T00:00:00Z',
  };

  beforeEach(async () => {
    transactionService = {
      getAllTransactions: vi.fn().mockReturnValue(of([])),
      createTransaction: vi.fn().mockReturnValue(of(sampleTransaction)),
      updateTransaction: vi.fn().mockReturnValue(of(sampleTransaction)),
      deleteTransaction: vi.fn().mockReturnValue(of(undefined)),
    };
    accountService = { getAllAccounts: vi.fn().mockReturnValue(of([])) };
    categoryService = { getAllCategories: vi.fn().mockReturnValue(of(categories)) };

    await TestBed.configureTestingModule({
      imports: [Transactions],
      providers: [
        { provide: TransactionService, useValue: transactionService },
        { provide: AccountService, useValue: accountService },
        { provide: CategoryService, useValue: categoryService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Transactions);
    component = fixture.componentInstance;
  });

  it('should create and load transactions/accounts/categories on init', () => {
    fixture.detectChanges();

    expect(transactionService.getAllTransactions).toHaveBeenCalled();
    expect(accountService.getAllAccounts).toHaveBeenCalled();
    expect(categoryService.getAllCategories).toHaveBeenCalled();
  });

  describe('availableCategories', () => {
    it('defaults to EXPENSE categories', () => {
      fixture.detectChanges();

      const names = component['availableCategories']().map((c) => c.name);
      expect(names).toEqual(['Comida']);
    });

    it('filters categories by the currently selected type', () => {
      fixture.detectChanges();

      component['selectedTransactionType'].set('INCOME');

      const names = component['availableCategories']().map((c) => c.name);
      expect(names.sort()).toEqual(['Freelance', 'Sueldo']);
    });

    it('resets categoryId to 0 when the transaction type changes', () => {
      fixture.detectChanges();
      component['transactionForm'].controls.categoryId.setValue(3);
      component['transactionForm'].controls.type.setValue('INCOME');

      component.onTransactionTypeChange();

      expect(component['selectedTransactionType']()).toBe('INCOME');
      expect(component['transactionForm'].controls.categoryId.value).toBe(0);
      const names = component['availableCategories']().map((c) => c.name);
      expect(names.sort()).toEqual(['Freelance', 'Sueldo']);
    });
  });

  describe('openEditModal', () => {
    it('preselects the type and category of the transaction being edited', () => {
      fixture.detectChanges();

      component.openEditModal(sampleTransaction);

      expect(component['selectedTransactionType']()).toBe('EXPENSE');
      expect(component['transactionForm'].controls.categoryId.value).toBe(3);
      expect(component['transactionForm'].controls.accountId.value).toBe(1);
      expect(component['isModalOpen']()).toBe(true);
    });
  });

  describe('saveTransaction', () => {
    it('does not call the service when the form is invalid', () => {
      fixture.detectChanges();
      component['transactionForm'].controls.accountId.setValue(0);

      component.saveTransaction();

      expect(transactionService.createTransaction).not.toHaveBeenCalled();
      expect(transactionService.updateTransaction).not.toHaveBeenCalled();
    });

    it('creates a new transaction when not editing', () => {
      fixture.detectChanges();
      component['transactionForm'].setValue({
        accountId: 1,
        categoryId: 3,
        amount: 500,
        type: 'EXPENSE',
        description: 'Almuerzo',
        date: '2026-03-15',
      });

      component.saveTransaction();

      expect(transactionService.createTransaction).toHaveBeenCalledWith({
        accountId: 1,
        categoryId: 3,
        amount: 500,
        type: 'EXPENSE',
        description: 'Almuerzo',
        date: '2026-03-15',
      });
      expect(component['isModalOpen']()).toBe(false);
    });

    it('sends null description when the field is blank', () => {
      fixture.detectChanges();
      component['transactionForm'].setValue({
        accountId: 1,
        categoryId: 3,
        amount: 500,
        type: 'EXPENSE',
        description: '   ',
        date: '2026-03-15',
      });

      component.saveTransaction();

      expect(transactionService.createTransaction).toHaveBeenCalledWith(
        expect.objectContaining({ description: null }),
      );
    });

    it('updates the transaction when editingTransactionId is set', () => {
      fixture.detectChanges();
      component.openEditModal(sampleTransaction);
      component['transactionForm'].controls.amount.setValue(600);

      component.saveTransaction();

      expect(transactionService.updateTransaction).toHaveBeenCalledWith(10, {
        categoryId: 3,
        amount: 600,
        description: 'Almuerzo',
        date: '2026-03-15',
      });
      expect(transactionService.createTransaction).not.toHaveBeenCalled();
    });
  });

  describe('confirmDelete', () => {
    it('does nothing when there is no transaction pending deletion', () => {
      fixture.detectChanges();

      component.confirmDelete();

      expect(transactionService.deleteTransaction).not.toHaveBeenCalled();
    });

    it('deletes the pending transaction and reloads the list', () => {
      fixture.detectChanges();
      component['deletingTransaction'].set(sampleTransaction);

      component.confirmDelete();

      expect(transactionService.deleteTransaction).toHaveBeenCalledWith(10);
      expect(component['deletingTransaction']()).toBeNull();
      // loadTransactions se llama de nuevo tras borrar (init + reload)
      expect(transactionService.getAllTransactions).toHaveBeenCalledTimes(2);
    });
  });
});
