import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoryExpensesChart } from './category-expenses-chart';

describe('CategoryExpensesChart', () => {
  let component: CategoryExpensesChart;
  let fixture: ComponentFixture<CategoryExpensesChart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryExpensesChart],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryExpensesChart);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
