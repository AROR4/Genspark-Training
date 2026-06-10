import { Component, inject, signal } from '@angular/core';
import { ProductService } from '../../Services/product.service';
import { ProductModel } from '../../Models/Product';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-products',
  imports: [RouterLink],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products {
  productService =inject(ProductService);
  products = signal<ProductModel[]>([]);
  isLoading=signal(false);
  constructor(){
    this.isLoading.set(true);
    this.productService.getProductData().subscribe({
      next :(response)=>{
        this.products.set(response);
        this.isLoading.set(false);
        console.log(this.products());
      },
      error: (error) => {
        console.error(error);
        this.isLoading.set(false);
      },
    })
  }
}
