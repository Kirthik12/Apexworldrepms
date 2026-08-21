import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminPropertyManagement } from './admin-property-management';

describe('AdminPropertyManagement', () => {
  let component: AdminPropertyManagement;
  let fixture: ComponentFixture<AdminPropertyManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AdminPropertyManagement],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminPropertyManagement);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
