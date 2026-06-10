import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../../Services/product.service';
import { ProductModel } from '../../Models/Product';

@Component({
  selector: 'app-product-details',
  imports: [],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css',
})
export class ProductDetails {
  productService =inject(ProductService);
  product =signal<ProductModel | null>(null);
  isLoading = signal(false);
  constructor(private route: ActivatedRoute) {
    this.isLoading.set(true);
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.productService.getProductById(+id).subscribe({
        next: (response) => {
          this.product.set(response);
          this.isLoading.set(false);
        },
        error: (error) => {
          console.error(error);
          this.isLoading.set(false);
        },
      });
    } else {
      console.error('Product ID is missing in the route parameters.');
      this.isLoading.set(false);
    }
  }
}
