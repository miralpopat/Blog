﻿using System.Linq;
using System.Web.Mvc;
using MvcMusicStore.Models;
using MvcMusicStore.ViewModels;
using System;
using Messages;

namespace MvcMusicStore.Controllers
{
    public class ShoppingCartController : Controller
    {
        MusicStoreEntities storeDB = new MusicStoreEntities();

        //
        // GET: /ShoppingCart/

        public ActionResult Index()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            // Set up our ViewModel
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()
            };

            // Return the view
            return View(viewModel);
        }

        // AF - we have to add this b/c our cart item doesn't show up right away, but we can show the album we just added
        public ActionResult AddedItemToCart(int id)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            // Retrieve the album from the database
            var addedAlbum = storeDB.Albums
                .Single(album => album.AlbumId == id);

            // Set up our ViewModel
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = new System.Collections.Generic.List<Cart>(),
                CartTotal = cart.GetTotal() + addedAlbum.Price
            };

            viewModel.CartItems.Add(new Cart { Album = addedAlbum, AlbumId = addedAlbum.AlbumId, Count = 1 });

            return View(viewModel);
        }

        //
        // GET: /Store/AddToCart/5

        public ActionResult AddToCart(int id)
        {

            // Retrieve the album from the database
            var addedAlbum = storeDB.Albums
                .Single(album => album.AlbumId == id);

            // Add it to the shopping cart
            var cart = ShoppingCart.GetCart(this.HttpContext);
            
            Helpers.ServiceAgent<IAddToCartCommand>.Send(
                c => 
                {
                    c.CartId = cart.GetCartId(this.HttpContext);
                    c.AlbumId = addedAlbum.AlbumId;
                });

            // Go back to the main store page for more shopping
            return RedirectToAction("AddedItemToCart", new { id = addedAlbum.AlbumId } );
        }

        //
        // AJAX: /ShoppingCart/RemoveFromCart/5

        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            // Remove the item from the cart
            var cart = ShoppingCart.GetCart(this.HttpContext);

            // Get the name of the album to display confirmation
            string albumName = storeDB.Carts
                .Single(item => item.RecordId == id).Album.Title;

            // Remove from cart. Note that for simplicity, we're 
            // removing all rather than decrementing the count.
            cart.RemoveFromCart(id);

            // Display the confirmation message
            var results = new ShoppingCartRemoveViewModel { 
                Message = Server.HtmlEncode(albumName) + 
                    " has been removed from your shopping cart.",
                CartTotal = cart.GetTotal(),
                CartCount = cart.GetCount(),
                DeleteId = id 
            };

            return Json(results);
        }

        //
        // GET: /ShoppingCart/CartSummary

        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            ViewData["CartCount"] = cart.GetCount();

            return PartialView("CartSummary");
        }
    }
}