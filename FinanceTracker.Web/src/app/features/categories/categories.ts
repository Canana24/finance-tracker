import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CategoryService } from '../../core/services/category.service';
import { Category, CategoryType } from '../../core/models/category.model';
import { Modal } from '../../shared/components/modal/modal';
import { LucideAngularModule, LucideIconData, Trash2 } from 'lucide-angular';

@Component({
  selector: 'app-categories',
  imports: [ReactiveFormsModule, Modal, LucideAngularModule],
  templateUrl: './categories.html',
  styleUrl: './categories.scss',
})
export class Categories implements OnInit {
  private categoryService = inject(CategoryService);
  private formBuilder = inject(FormBuilder);

  protected readonly categories = signal<Category[]> ([]);
  protected readonly isLoading = signal(true);

  protected readonly isModalOpen = signal(false);
  protected readonly editingCategoryId = signal<number | null> (null);
  protected readonly isSaving = signal(false);
  protected readonly deletingCategory = signal<Category | null>(null);
  protected readonly trashIcon = Trash2;

  protected readonly categoryForm = this.formBuilder.nonNullable.group({
    name: ['',[Validators.required, Validators.minLength(2)]],
    type: ['EXPENSE' as CategoryType, [Validators.required]],
  });

  ngOnInit(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.categoryService.getAllCategories().subscribe ({
      next: (data) => {
        this.categories.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  openCreateModal(): void {
    this.editingCategoryId.set(null);
    this.categoryForm.reset({name: '', type: 'EXPENSE'});
    this.isModalOpen.set(true);
  }

  openEditModal(category: Category): void {
    this.editingCategoryId.set(category.id);
    this.categoryForm.setValue({name: category.name, type: category.type});
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveCategory(): void {
    if (this.categoryForm.invalid){
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    const formValue = this.categoryForm.getRawValue();
    const editingId = this.editingCategoryId();

    if(editingId === null) {
      this.categoryService.createCategory({ name: formValue.name, type: formValue.type, icon: null})
      .subscribe({
        next: () => this.onSaveSuccess(),
        error: () => this.isSaving.set(false),      
      });
    } else
    {
      this.categoryService.updateCategory(editingId, {name: formValue.name, icon: null})
      .subscribe({
        next: () => this.onSaveSuccess(),
        error: () => this.isSaving.set(false),
      });
    }
  }

  private onSaveSuccess (): void {
    this.isSaving.set(false);
    this.closeModal();
    this.loadCategories();
  }

  askDeleteConfirmation(category:Category): void {
    this.deletingCategory.set(category);
  }

  cancelDelete(): void {
    const category = this.deletingCategory();
    if (category === null){
      return;
    }
  }

  confirmDelete(): void {
    const category = this.deletingCategory();
    if (category === null) {
      return;
    }

    this.isSaving.set(true);
    this.categoryService.deleteCategory(category.id).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.deletingCategory.set(null);
        this.loadCategories();
      },
      error: () => this.isSaving.set(false),
    });
  }
}
