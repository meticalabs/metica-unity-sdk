using System;
using System.Collections.Generic;
using System.Linq;
using Metica.Unity;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeticaUnitySDK.Assets.Editor
{
    public class MeticaSettings : EditorWindow
    {
        private List<Offer> _offers = new();

        [SerializeField] private string _appId = "";

        [SerializeField] private string _userId = "";

        [SerializeField] private string _apiKey = "";

        [SerializeField] private string _ingestionEndpoint = "";

        [SerializeField] private string _offersEndpoint = "";

        [SerializeField] private List<string> _placements = new();

        private int _selectedPanel = 0;
        private Offer _selectedOffer;
        private int _selectedPlacement = -1;

        private ListView _offersListView;
        private VisualElement _detailsView;

        private TextField _placementInput;
        private Button _deleteButton;
        private Button _deletePlacementButton;

        private readonly MeticaEditorAPI _editorAPI = new();

        [MenuItem("Window/Metica Settings")]
        public static void ShowWindow()
        {
            EditorWindow wnd = GetWindow<MeticaSettings>("Metica Settings");
            wnd.titleContent = new GUIContent("Metica Settings");
        }

        public MeticaSettings()
        {
            _placements.Add("main");
        }

        void OnEnable()
        {
            PopulateView();
        }

        private void PopulateView()
        {
            rootVisualElement.Clear();
            PopulateToolbar();
            if (_selectedPanel == 0)
            {
                PopulateSettingsPanel();
            }
            else if (_selectedPanel == 1)
            {
                PopulateOffersPanel();
            }
            else
            {
                PopulateEventsPanel();
            }
        }

        private void PopulateToolbar()
        {
            var toolbar = new Toolbar()
            {
                style =
                {
                    marginBottom = 10
                }
            };
            rootVisualElement.Add(toolbar);

            var settingsButton = new ToolbarButton(() =>
            {
                _selectedPanel = 0;
                PopulateView();
            })
            {
                text = "Settings",
                style =
                {
                    backgroundColor = _selectedPanel == 0 ? Color.gray : Color.clear
                }
            };
            toolbar.Add(settingsButton);

            var offersButton = new ToolbarButton(() =>
            {
                _selectedPanel = 1;
                PopulateView();
            })
            {
                text = "Offers",
                style =
                {
                    backgroundColor = _selectedPanel == 1 ? Color.gray : Color.clear
                }
            };
            toolbar.Add(offersButton);

            var eventsButton = new ToolbarButton(() =>
            {
                _selectedPanel = 2;
                PopulateView();
            })
            {
                text = "Events",
                style =
                {
                    backgroundColor = _selectedPanel == 2 ? Color.gray : Color.clear,
                    unityFontStyleAndWeight = _selectedPanel == 2 ? FontStyle.Bold : FontStyle.Normal
                }
            };
            toolbar.Add(eventsButton);
        }

        private void FetchOffers()
        {
            _editorAPI.GetOffersInEditor(new[] { "main" }, result =>
            {
                if (result.Error != null)
                {
                    Debug.LogError("Error while fetching offers: " + result.Error);
                }
                else
                {
                    foreach (var p in result.Result.placements.Keys)
                    {
                        var offers = result.Result.placements[p];

                        Debug.Log($"Placement {p} offers: {JsonConvert.SerializeObject(offers)}");
                    }
                }
            });
        }

        private void PopulateOffersPanel()
        {
            Debug.Log("Offers count: " + _offers.Count);
            if (_offers.Count == 0)
            {
                _offers.Add(new Offer
                {
                    offerId = "123",
                    customPayload = "{}",
                    price = 1.23,
                    expirationTime = "2024-05-01T00:00:00Z"
                });
            }

            var offersParentView = rootVisualElement;
            // rootVisualElement.Add(offersParentView);

            var placementOptions = new DropdownField("Placements", _placements, 0);
            offersParentView.Add(placementOptions);

            // Fetch Button
            var fetchButton = new Button(() => FetchOffers()) { text = "Fetch Offers" };
            fetchButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            offersParentView.Add(fetchButton);

            var panelTitle = new TextElement()
            {
                text =
                    "<size=14>Eligible Offers</size><br><size=12>The list of eligible offers for the user. Click on an offer to see its details.</size>",
                style =
                {
                    fontSize = 13,
                    unityFontStyleAndWeight = FontStyle.Normal,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginTop = 20,
                    marginBottom = 20
                }
            };

            offersParentView.Add(panelTitle);

            var mainSplitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            offersParentView.Add(mainSplitView);

            // ListView for offers
            _offersListView = new ListView(_offers, itemHeight: 20, makeItem: () => new Label(),
                bindItem: (item, index) => { (item as Label).text = $"Offer - {_offers[index].offerId}"; })
            {
                selectionType = SelectionType.Single,
                showBorder = true,
                headerTitle = "Offers",
                style =
                {
                    height = _offers.Count * 20
                }
            };
            _offersListView.onSelectionChange += OnOfferSelectionChange;
            mainSplitView.Add(_offersListView);

            _offersListView.Rebuild();

            // Details panel
            _detailsView = new ScrollView(ScrollViewMode.Vertical);
            mainSplitView.Add(_detailsView);
            mainSplitView.Add(_detailsView);
        }

        private void PopulateSettingsPanel()
        {
            var scrollPanel = new ScrollView(ScrollViewMode.Vertical);
            rootVisualElement.Add(scrollPanel);

            var appId = new TextField("Application ID");
            appId.value = _appId;
            appId.RegisterValueChangedCallback(evt =>
            {
                _editorAPI.AppId = evt.newValue;
                _appId = evt.newValue;
            });

            var apiKey = new TextField("API Key", 32, false, true, '*');
            apiKey.value = _apiKey;
            apiKey.RegisterValueChangedCallback(evt =>
            {
                _apiKey = evt.newValue;
                _editorAPI.APIKey = evt.newValue;
            });

            var userId = new TextField("User ID");
            userId.value = _userId;
            userId.RegisterValueChangedCallback(evt =>
            {
                _editorAPI.UserId = evt.newValue;
                _userId = evt.newValue;
            });

            var ingestionEndpoint = new TextField("Ingestion Endpoint");
            ingestionEndpoint.value = _ingestionEndpoint;
            ingestionEndpoint.RegisterValueChangedCallback(evt =>
            {
                _editorAPI.IngestionEndpoint = evt.newValue;
                _ingestionEndpoint = evt.newValue;
            });

            var offersEndpoint = new TextField("Offers Endpoint");
            offersEndpoint.value = _offersEndpoint;
            offersEndpoint.RegisterValueChangedCallback(evt =>
            {
                _editorAPI.OffersEndpoint = evt.newValue;
                _offersEndpoint = evt.newValue;
            });

            scrollPanel.Add(appId);
            scrollPanel.Add(apiKey);
            scrollPanel.Add(userId);
            scrollPanel.Add(ingestionEndpoint);
            scrollPanel.Add(offersEndpoint);

            // var label = new Label("Placements")
            // {
            //     style =
            //     {
            //         marginTop = 10
            //     }
            // };
            // scrollPanel.Add(label);

            PopulatePlacementsUI(scrollPanel);
        }

        private void PopulateOfferDetailsPanel()
        {
            // Offer Details
            var idField = new TextField("ID");
            var payloadField = new TextField("Payload", 4096, true, false, '*');
            var expirationTimeField = new TextField("Expiration Date");
            var priceField = new DoubleField("Price");
            var creativeId = new TextField("Creative ID");
            var currencyId = new TextField("Currency ID");
            var iap = new TextField("IAP");
            var creativeOverride = new TextField("Creative Override");
            var itemsList = new ListView(_selectedOffer.items, itemHeight: 20, makeItem: () => new Label(),
                bindItem: (item, index) =>
                {
                    (item as Label).text =
                        $"id: {_selectedOffer.items[index].id} - copies: {_selectedOffer.items[index].quantity}";
                })
            {
                style = { marginBottom = 10 }
            };
            _detailsView.Add(idField);
            _detailsView.Add(payloadField);
            _detailsView.Add(expirationTimeField);
            _detailsView.Add(priceField);
            _detailsView.Add(currencyId);
            _detailsView.Add(iap);
            _detailsView.Add(creativeId);
            _detailsView.Add(creativeOverride);
            _detailsView.Add(new Label("Items")
            {
                style = { marginTop = 10, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold }
            });
            _detailsView.Add(itemsList);

            idField.value = _selectedOffer.offerId;
            payloadField.value = _selectedOffer.customPayload;
            expirationTimeField.value = _selectedOffer.expirationTime;
            priceField.value = _selectedOffer.price ?? 0.0;
            creativeId.value = _selectedOffer.creativeId;
            creativeOverride.value = _selectedOffer.creativeOverride;
            currencyId.value = _selectedOffer.currencyId;
            iap.value = _selectedOffer.iap ?? "";

            var updateButton = new Button(() => Debug.Log("updating...")) { text = "Update" };
            _detailsView.Add(updateButton);

            _deleteButton = new Button(() => DeleteSelectedOffer()) { text = "Delete" };
            _detailsView.Add(_deleteButton);

            var displayLogFoldout = new Foldout()
            {
                text = "Display Log",
                style =
                {
                    marginTop = 10
                }
            };
            _detailsView.Add(displayLogFoldout);

            var displayLogEntries = _editorAPI.GetDisplayLog(_selectedOffer.offerId)
                .Where(entry => entry.placementId == _placements[_selectedPlacement]).ToList();
            var displayLogList = new ListView(displayLogEntries, itemHeight: 20, makeItem: () => new Label(),
                bindItem: (item, index) =>
                {
                    (item as Label).text =
                        $"Date: {displayLogEntries[index].displayedOn}";
                })
            {
                style = { marginBottom = 10 }
            };
            displayLogFoldout.Add(displayLogList);

            // Bind change listeners
            idField.RegisterValueChangedCallback(evt =>
            {
                if (_selectedOffer != null) _selectedOffer.offerId = evt.newValue;
            });
            payloadField.RegisterValueChangedCallback(evt =>
            {
                if (_selectedOffer != null) _selectedOffer.customPayload = evt.newValue;
            });
            expirationTimeField.RegisterValueChangedCallback(evt =>
            {
                if (_selectedOffer != null) _selectedOffer.expirationTime = evt.newValue;
            });
            creativeId.RegisterValueChangedCallback(evt =>
            {
                if (_selectedOffer != null) _selectedOffer.creativeId = evt.newValue;
            });
            currencyId.RegisterValueChangedCallback(evt =>
            {
                if (_selectedOffer != null) _selectedOffer.currencyId = evt.newValue;
            });
            iap.RegisterValueChangedCallback(evt =>
            {
                if (_selectedOffer != null) _selectedOffer.iap = evt.newValue;
            });
            creativeOverride.RegisterValueChangedCallback(evt =>
            {
                if (_selectedOffer != null) _selectedOffer.creativeOverride = evt.newValue;
            });
            priceField.RegisterValueChangedCallback(evt =>
            {
                if (_selectedOffer != null)
                {
                    _selectedOffer.price = evt.newValue;
                }
            });
        }

        private void PopulatePlacementsUI(VisualElement parent)
        {
            var group = new GroupBox("Placements")
            {
                style =
                {
                    marginTop = 10
                }
            };
            parent.Add(group);

            var placements = new ListView(_placements, itemHeight: 20, makeItem: () =>
                {
                    var txt = new TextField();
                    txt.RegisterValueChangedCallback(evt =>
                    {
                        var idx = _placements.IndexOf(evt.previousValue);
                        if (idx >= 0)
                        {
                            _placements[idx] = evt.newValue;
                        }
                    });
                    return txt;
                },
                bindItem: (item, index) => { (item as TextField).value = _placements[index]; })
            {
                selectionType = SelectionType.Single,
                showBorder = true,
                style =
                {
                    flexGrow = 1
                }
            };
            placements.onSelectionChange += OnPlacementSelectionChange;

            // Input field for adding or editing items
            _placementInput = new TextField
            {
                style =
                {
                    marginTop = 10,
                    marginBottom = 10
                }
            };

            // Add button
            var addButton = new Button(() =>
            {
                _placements.Add(_placementInput.value);
                _placementInput.value = "";
                placements.Rebuild();
            })
            {
                text = "Add"
            };

            // Remove button
            _deletePlacementButton = new Button(() =>
            {
                if (_selectedPlacement >= 0 && _selectedPlacement < _placements.Count)
                {
                    _placements.RemoveAt(_selectedPlacement);
                    placements.Rebuild();
                    ClearPlacementSelection();
                }
            })
            {
                text = "Remove",
                style = { marginTop = 5 }
            };
            _deletePlacementButton.SetEnabled(false); // Initially disabled

            group.Add(placements);
            group.Add(_deletePlacementButton);

            var addNew = new Foldout() { text = "Add new" };
            group.Add(addNew);

            // group.Add(new Label("Add new placement:"));
            addNew.Add(_placementInput);
            addNew.Add(addButton);
        }

        private void PopulateEventsPanel()
        {
            var eventsJson = new TextField("Event Payload", 4096, true, false, '*');
            rootVisualElement.Add(eventsJson);
            var submitButton = new Button(() =>
            {
                try
                {
                    var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventsJson.value);
                    _editorAPI.Init();
                    MeticaAPI.LogUserEvent(json);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while submitting event: " + e);
                }
            }) { text = "Submit Event" };
            rootVisualElement.Add(submitButton);
        }

        private void OnPlacementSelectionChange(IEnumerable<object> selection)
        {
            var selectedItems = new List<object>(selection);
            if (selectedItems.Count > 0)
            {
                var selectedValue = selectedItems[0] as string;
                _selectedPlacement = _placements.IndexOf(selectedValue);
                _placementInput.value = selectedValue;
                _deletePlacementButton.SetEnabled(true);
            }
            else
            {
                ClearPlacementSelection();
            }
        }

        private void ClearPlacementSelection()
        {
            _selectedPlacement = -1;
            _placementInput.value = "";
            _deletePlacementButton.SetEnabled(false);
        }

        private void OnOfferSelectionChange(IEnumerable<object> selection)
        {
            ClearSelectedOfferDetails();

            if (selection is List<object> list && list.Count > 0)
            {
                _selectedOffer = list[0] as Offer;
                if (_selectedOffer == null) return;

                PopulateOfferDetailsPanel();
                _deleteButton.SetEnabled(true);
            }
            else
            {
                _deleteButton.SetEnabled(false);
                _detailsView.Clear();
            }
        }

        private void ClearSelectedOfferDetails()
        {
            _detailsView.Clear();
        }

        private void DeleteSelectedOffer()
        {
            if (_selectedOffer != null)
            {
                _offers.Remove(_selectedOffer);
                _offersListView.Rebuild();
                _selectedOffer = null;
                _deleteButton.SetEnabled(false);
                ClearSelectedOfferDetails();
            }
        }
    }
}