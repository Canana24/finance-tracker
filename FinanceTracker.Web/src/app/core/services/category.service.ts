import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../models/category.model';

@Injectable({providedIn: 'root'})
export class CategoryService {
    
    private http = inject(HttpClient);
    private readonly baseUrl = `${environment.apiUrl}/Category`;

    getAllCategories() {
        return this.http.get<Category[]>(this.baseUrl);
    }

    createCategory(category: CreateCategoryRequest){
        return this.http.post<Category>(this.baseUrl, category);
    }

    updateCategory(categoryId: number, category: UpdateCategoryRequest) {
        return this.http.put<Category>(`${this.baseUrl}/${categoryId}`, category);
    }
    
    deleteCategory(categoryId: number){
        return this.http.delete<void>(`${this.baseUrl}/${categoryId}`);
    }
}