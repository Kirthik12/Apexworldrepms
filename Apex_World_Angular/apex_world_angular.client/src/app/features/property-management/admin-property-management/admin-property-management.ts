import {
  Component,
  AfterViewInit,
  ViewEncapsulation,
  OnInit,
  ChangeDetectorRef,
} from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  FormsModule,
  ReactiveFormsModule,
} from '@angular/forms';
import { PropertyService } from '../../../core/services/property.service';
import { ToastService } from '../../../core/services/toast.service';
import { PropertyDto, PropertyUpdateDto } from '../../../core/models/property.model';
import { environment } from '../../../../environments/environment';
import { AdminHeader } from '../../../shared/components/admin-header/admin-header';
import { NgStyle, NgIf, NgFor, NgClass, DecimalPipe } from '@angular/common';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-admin-property-management',
  templateUrl: './admin-property-management.html',
  styleUrls: ['./admin-property-management.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [
    AdminHeader,
    NgStyle,
    FormsModule,
    NgIf,
    NgFor,
    NgClass,
    ReactiveFormsModule,
    DecimalPipe,
    PaginationComponent,
  ],
})
export class AdminPropertyManagement implements OnInit, AfterViewInit {
  backendUrl = environment.apiUrl.replace('/api/v1', '');

  // ── Table state ────────────────────────────────
  properties: PropertyDto[] = [];

  // ── Pagination state ──────────────────────────────────────────────
  totalItems: number = 0;
  pageNumber: number = 1;
  pageSize: number = 7;
  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize) || 1;
  }
  allFetchedProperties: PropertyDto[] = [];
  activeFilter: string = 'All';

  // ── Dashboard counters ────────────────────────────────────────────
  totalProperties: number = 0;
  availableProperties: number = 0;
  bookedProperties: number = 0;
  soldProperties: number = 0;

  // ── Active tab ────────────────────────────────────────────────────
  activeTab: string = 'all';
  policyAccepted: boolean = false;

  // ── State for modals ──────────────────────────────────────────────
  propertyToDeleteId: number | null = null;
  propertyToEditId: number | null = null;

  showPolicyModal: boolean = false;
  showAddConfirmModal: boolean = false;
  selectedPropertyIdForStatus: number | null = null;
  showDeleteModal: boolean = false;
  showEditModal: boolean = false;
  showStatusModal: boolean = false;
  showViewModal: boolean = false;
  viewProperty: PropertyDto | null = null;

  // ── Forms ────────────────────────────────────────────────────────
  addPropertyForm: FormGroup;
  editPropertyForm: FormGroup;
  statusForm: FormGroup;
  selectedFile: File | null = null;

  constructor(
    private fb: FormBuilder,
    private propertyService: PropertyService,
    private cdr: ChangeDetectorRef,
    private toastService: ToastService
  ) {
    this.addPropertyForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      price: [0, [Validators.required, Validators.min(1)]],
      category: ['Apartment', Validators.required],
      address: ['', Validators.required],
      carpetArea: [0],
      facing: ['North'],
      projectName: [''],
      bedrooms: [0],
      bathrooms: [0],
      areaSize: [0],
      furnishing: ['Unfurnished'],
      totalFloors: [1],
      maintenance: [0],
      carParking: [0],
    });

    this.editPropertyForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      price: [0, Validators.required],
      projectName: [''],
      furnishing: ['Unfurnished'],
      totalFloors: [1],
      maintenance: [0],
    });

    this.statusForm = this.fb.group({
      status: ['Pending', Validators.required],
      isAvailable: [true],
    });
  }

  ngOnInit(): void {
    this.loadProperties();
  }

  ngAfterViewInit(): void {
    // Legacy DOM bindings removed
  }

  // ── API Interactions ──────────────────────────────────────────────
  loadProperties(): void {
    this.propertyService.getAllPropertiesAdmin(1, 1000).subscribe((res) => {
      if (res && res.data) {
        this.allFetchedProperties = res.data.items;
        this.totalProperties = this.allFetchedProperties.length;
        this.availableProperties = this.allFetchedProperties.filter(
          (p) => p.status === 'Available' || p.isAvailable,
        ).length;
        this.bookedProperties = this.allFetchedProperties.filter(
          (p) => p.status === 'Booked',
        ).length;
        this.soldProperties = this.allFetchedProperties.filter((p) => p.status === 'Sold').length;

        this.applyFilter();
      }
    });
  }

  applyFilter(): void {
    let filtered = this.allFetchedProperties;

    if (this.activeFilter === 'Available') {
      filtered = filtered.filter((p) => p.status === 'Available' || p.isAvailable);
    } else if (this.activeFilter === 'Booked') {
      filtered = filtered.filter((p) => p.status === 'Booked');
    } else if (this.activeFilter === 'Sold') {
      filtered = filtered.filter((p) => p.status === 'Sold');
    }

    this.totalItems = filtered.length;

    const startIndex = (this.pageNumber - 1) * this.pageSize;
    this.properties = filtered.slice(startIndex, startIndex + this.pageSize);

    this.cdr.detectChanges();
  }

  filterBy(status: string): void {
    this.activeFilter = status;
    this.pageNumber = 1;
    this.applyFilter();
  }

  changePage(newPage: number): void {
    const totalPages = Math.ceil(this.totalItems / this.pageSize);
    if (newPage < 1 || (totalPages > 0 && newPage > totalPages)) return;
    this.pageNumber = newPage;
    this.applyFilter();
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
    this.applyFilter();
  }

  // ── Tab Switcher ──────────────────────────────────────────────────
  switchToTab(target: string): void {
    if (target === 'add' && !this.policyAccepted) {
      this.showPolicyModal = true;
      return;
    }
    this.activeTab = target;
  }

  acceptPolicy(accepted: boolean): void {
    if (accepted) {
      this.policyAccepted = true;
      this.showPolicyModal = false;
      this.switchToTab('add');
    } else {
      this.showPolicyModal = false;
    }
  }

  // ── View Property Details ─────────────────────────────────────────
  viewPropertyDetails(id: number): void {
    this.viewProperty = null;
    this.showViewModal = true;
    this.propertyService.getAdminPropertyById(id).subscribe({
      next: (res) => {
        this.viewProperty = res.data ?? (res as any);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load property details', err);
        this.showViewModal = false;
        this.toastService.error('❌ Failed to load property details.');
        this.cdr.detectChanges();
      },
    });
  }

  closeViewModal(): void {
    this.showViewModal = false;
    this.viewProperty = null;
  }

  // ── Add Property Form ─────────────────────────────────────────────
  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  requestAddProperty(): void {
    if (this.addPropertyForm.invalid) {
      this.toastService.error('❌ Please fill all required fields.');
      return;
    }
    this.showAddConfirmModal = true;
  }

  submitAddProperty(): void {
    this.showAddConfirmModal = false;

    const formData = new FormData();
    const formValues = this.addPropertyForm.value;

    // Append all form values to FormData
    Object.keys(formValues).forEach((key) => {
      formData.append(key, formValues[key]);
    });

    if (this.selectedFile) {
      formData.append('ImageFile', this.selectedFile, this.selectedFile.name);
    }

    this.propertyService.addProperty(formData).subscribe({
      next: () => {
        this.addPropertyForm.reset({
          category: 'Apartment',
          facing: 'North',
          furnishing: 'Unfurnished',
        });
        this.selectedFile = null;
        this.switchToTab('all');
        this.toastService.success('Property added successfully.');
        this.cdr.detectChanges();
        setTimeout(() => this.loadProperties(), 300);
      },
      error: (err) => {
        this.toastService.error(
          '❌ Failed to add property: ' + (err.error?.message || err.message)
        );
        this.cdr.detectChanges();
      },
    });
  }

  // ── Delete Property ─────────────────────────────────────────────
  promptDelete(id: number): void {
    this.propertyToDeleteId = id;
    this.showDeleteModal = true;
  }

  executeDelete(): void {
    if (this.propertyToDeleteId === null) return;

    this.propertyService.deleteProperty(this.propertyToDeleteId).subscribe({
      next: () => {
        this.toastService.success('Property deleted successfully.');
        this.loadProperties();
        this.propertyToDeleteId = null;
        this.showDeleteModal = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.toastService.error('❌ Failed to delete property.');
        this.cdr.detectChanges();
      },
    });
  }

  // ── Edit Property ───────────────────────────────────────────────
  promptEdit(id: number): void {
    this.propertyToEditId = id;
    const prop = this.properties.find((p) => p.id === id);
    if (prop) {
      this.editPropertyForm.patchValue({
        title: prop.title,
        description: prop.description,
        price: prop.price,
        projectName: prop.projectName,
        furnishing: prop.furnishing,
        totalFloors: prop.totalFloors,
        maintenance: prop.maintenance,
      });
      this.showEditModal = true;
    }
  }

  submitEditProperty(): void {
    if (this.propertyToEditId === null || this.editPropertyForm.invalid) return;

    const updateData: PropertyUpdateDto = this.editPropertyForm.value;

    this.propertyService.updateProperty(this.propertyToEditId, updateData).subscribe({
      next: () => {
        this.toastService.success('Property updated successfully.');
        this.loadProperties();
        this.propertyToEditId = null;
        this.showEditModal = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.toastService.error('❌ Failed to update property.');
        this.cdr.detectChanges();
      },
    });
  }

  // ── Status Property ───────────────────────────────────────────────
  promptStatusUpdate(id: number): void {
    this.selectedPropertyIdForStatus = id;
    const prop = this.properties.find((p) => p.id === id);
    if (prop) {
      this.statusForm.patchValue({
        status: prop.status,
        isAvailable: prop.isAvailable,
      });
      this.showStatusModal = true;
    }
  }

  closeStatusModal(): void {
    this.showStatusModal = false;
    this.selectedPropertyIdForStatus = null;
  }

  submitStatusUpdate(): void {
    if (this.selectedPropertyIdForStatus === null) {
      this.toastService.error('❌ Property ID is missing.');
      return;
    }
    if (this.statusForm.invalid) {
      this.toastService.error('❌ Form is invalid.');
      return;
    }

    this.toastService.info('⏳ Updating status...');

    this.propertyService
      .updatePropertyStatus(this.selectedPropertyIdForStatus, this.statusForm.value)
      .subscribe({
        next: (res) => {
          this.toastService.success('Property status updated successfully.');
          this.loadProperties();
          this.closeStatusModal();
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.toastService.error(
            '❌ Failed to update status: ' + (err.message || 'Unknown error')
          );
          this.cdr.detectChanges();
        },
      });
  }

  // ── Helpers ───────────────────────────────────────────────────────
  getCoverImage(property: PropertyDto): string {
    if (property && property.images && property.images.length > 0) {
      const img = property.images[0].imageUrl;
      return img.startsWith('http') ? img : `${this.backendUrl}${img}`;
    }
    return '/assets/images/no_image_icon.png';
  }

  getBadgeClass(status: string): string {
    if (!status) return 'badge-pending';
    const s = status.toLowerCase();
    if (s === 'available' || s === 'approved') return 'badge-available';
    if (s === 'sold' || s === 'unavailable') return 'badge-sold';
    if (s === 'booked') return 'badge-booked';
    return 'badge-pending';
  }
}
