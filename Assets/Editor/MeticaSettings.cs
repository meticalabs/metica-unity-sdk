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

        [SerializeField] private string appId = "";

        [SerializeField] private string userId = "";

        [SerializeField] private string apiKey = "";

        [SerializeField] private string ingestionEndpoint = "";

        [SerializeField] private string offersEndpoint = "";

        [SerializeField] private List<string> placements = new();

        [SerializeField] private int selectedEventTemplate;

        [SerializeField] private string customEventPayload = "";

        [SerializeField] private string eventOfferId = "";
        [SerializeField] private string eventPlacementId = "";
        [SerializeField] private double eventAmount;
        [SerializeField] private string eventCurrency = "USD";
        [SerializeField] private string eventInteractionType = "";

        private int _selectedPanel;
        private Offer _selectedOffer;

        [SerializeField] private int selectedPlacement = 0;
        [SerializeField] private int selectedOffersPlacement = 0;
        [SerializeField] private string overrideUserProperties;
        [SerializeField] private string deviceInfoAppStore;
        [SerializeField] private string deviceInfoTimezone;
        [SerializeField] private string deviceInfoLocale;
        [SerializeField] private string deviceInfoAppVersion;

        private ListView _offersListView;
        private VisualElement _detailsView;

        private TextField _placementInput;
        private Button _deleteButton;
        private Button _deletePlacementButton;

        private readonly MeticaEditorAPI _editorAPI = new();

        [MenuItem("Window/Metica")]
        public static void ShowWindow()
        {
            EditorWindow wnd = GetWindow<MeticaSettings>("Metica");
            wnd.titleContent = new GUIContent("Metica");
        }

        public MeticaSettings()
        {
            placements.Add("main");
        }

        void OnEnable()
        {
            PopulateView();
            _editorAPI.AppId = appId;
            _editorAPI.APIKey = apiKey;
            _editorAPI.UserId = userId;
            _editorAPI.IngestionEndpoint = ingestionEndpoint;
            _editorAPI.OffersEndpoint = offersEndpoint;
        }

        private void PopulateView()
        {
            rootVisualElement.Clear();
            PopulateToolbar();
            switch (_selectedPanel)
            {
                case 0:
                    PopulateSettingsPanel();
                    break;
                case 1:
                    PopulateOffersPanel();
                    break;
                default:
                    PopulateEventsPanel();
                    break;
            }
        }

        private void PopulateToolbar()
        {
            var toolbar = new Toolbar
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
            var userProperties = overrideUserProperties != null
                ? JsonConvert.DeserializeObject<Dictionary<string, object>>(overrideUserProperties)
                : null;

            var deviceInfo = new DeviceInfo()
            {
                store = deviceInfoAppStore,
                locale = deviceInfoLocale,
                timezone = deviceInfoTimezone,
                appVersion = deviceInfoAppVersion
            };
            
            _editorAPI.GetOffersInEditor(new[] { placements[selectedOffersPlacement] }, result =>
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
                            _offers = offers;
                            Debug.Log($"Placement {p} offers: {JsonConvert.SerializeObject(offers)}");
                        }
                    }

                    if (_offers.Count > 0)
                    {
                        _selectedOffer = _offers[0];
                    }

                    PopulateView();
                },
                userProperties,
                deviceInfo);
        }

        private Foldout CreateDeviceInfoFoldout()
        {
            var deviceInfoFoldout = new Foldout { text = "Override Device Info" };
            var appstoreField = new DropdownField("App Store",
                new List<string>() { StoreTypeEnum.GooglePlayStore.ToString(), StoreTypeEnum.AppStore.ToString() }, 0);
            appstoreField.value = deviceInfoAppStore;
            appstoreField.RegisterValueChangedCallback(evt => deviceInfoAppStore = evt.newValue);

            var timezoneField = new TextField("Timezone (e.g. +02:00)");
            timezoneField.value = deviceInfoTimezone;
            timezoneField.RegisterValueChangedCallback(evt => deviceInfoTimezone = evt.newValue);

            var localeField = new TextField("Locale (e.g. en_US)");
            localeField.value = deviceInfoLocale;
            localeField.RegisterValueChangedCallback(evt => deviceInfoLocale = evt.newValue);

            var appVersionField = new TextField("App Version (semantic versioning format)");
            appVersionField.value = deviceInfoAppVersion;
            appVersionField.RegisterValueChangedCallback(evt => deviceInfoAppVersion = evt.newValue);

            deviceInfoFoldout.Add(appstoreField);
            deviceInfoFoldout.Add(timezoneField);
            deviceInfoFoldout.Add(localeField);
            deviceInfoFoldout.Add(appVersionField);

            return deviceInfoFoldout;
        }

        private void PopulateOffersPanel()
        {
            var offersParentView = rootVisualElement;

            var placementOptions = new DropdownField("Placements", placements, 0);
            placementOptions.RegisterValueChangedCallback(evt =>
                selectedOffersPlacement = placements.IndexOf(evt.newValue));
            placementOptions.index = selectedOffersPlacement;

            offersParentView.Add(placementOptions);

            var userPropsFoldout = new Foldout { text = "Override User Properties" };
            var userPropsTextField = new TextField("Properties JSON", 4096, true, false, '*');
            userPropsTextField.value = overrideUserProperties;
            userPropsTextField.RegisterValueChangedCallback(evt => overrideUserProperties = evt.newValue);
            userPropsFoldout.Add(userPropsTextField);
            offersParentView.Add(userPropsFoldout);

            var deviceInfoFoldout = CreateDeviceInfoFoldout();
            offersParentView.Add(deviceInfoFoldout);

            // Fetch Button
            var fetchButton = new Button(() => FetchOffers()) { text = "Fetch Offers" };
            fetchButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            offersParentView.Add(fetchButton);

            var panelTitle = new TextElement
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


            _offersListView.selectionChanged += OnOfferSelectionChange;
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

            var appIdField = new TextField("Application ID");
            appIdField.value = appId;
            appIdField.RegisterValueChangedCallback(evt =>
            {
                _editorAPI.AppId = evt.newValue;
                appId = evt.newValue;
            });

            var apiKeyField = new TextField("API Key", 32, false, true, '*');
            apiKeyField.value = apiKey;
            apiKeyField.RegisterValueChangedCallback(evt =>
            {
                apiKey = evt.newValue;
                _editorAPI.APIKey = evt.newValue;
            });

            var userIdField = new TextField("User ID");
            userIdField.value = userId;
            userIdField.RegisterValueChangedCallback(evt =>
            {
                _editorAPI.UserId = evt.newValue;
                userId = evt.newValue;
            });

            var ingestionEndpointField = new TextField("Ingestion Endpoint");
            ingestionEndpointField.value = ingestionEndpoint;
            ingestionEndpointField.RegisterValueChangedCallback(evt =>
            {
                _editorAPI.IngestionEndpoint = evt.newValue;
                ingestionEndpoint = evt.newValue;
            });

            var offersEndpointField = new TextField("Offers Endpoint");
            offersEndpointField.value = offersEndpoint;
            offersEndpointField.RegisterValueChangedCallback(evt =>
            {
                _editorAPI.OffersEndpoint = evt.newValue;
                offersEndpoint = evt.newValue;
            });

            scrollPanel.Add(appIdField);
            scrollPanel.Add(apiKeyField);
            scrollPanel.Add(userIdField);
            scrollPanel.Add(ingestionEndpointField);
            scrollPanel.Add(offersEndpointField);

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
            payloadField.value = JsonConvert.SerializeObject(_selectedOffer.customPayload, Formatting.Indented);
            expirationTimeField.value = _selectedOffer.expirationTime;
            priceField.value = _selectedOffer.price ?? 0.0;
            creativeId.value = _selectedOffer.creativeId;
            creativeOverride.value = JsonConvert.SerializeObject(_selectedOffer.creativeOverride, Formatting.Indented);
            currencyId.value = _selectedOffer.currencyId;
            iap.value = _selectedOffer.iap ?? "";

            var updateButton = new Button(() => Debug.Log("updating...")) { text = "Update" };
            // _detailsView.Add(updateButton);

            _deleteButton = new Button(() => DeleteSelectedOffer()) { text = "Delete" };
            // _detailsView.Add(_deleteButton);

            var displayLogFoldout = new Foldout
            {
                text = "Past Displays",
                style =
                {
                    marginTop = 10
                }
            };
            _detailsView.Add(displayLogFoldout);

            var displayLogEntries = _editorAPI.GetDisplayLog(_selectedOffer.offerId)
                .Where(entry => entry.placementId == placements[selectedOffersPlacement]).ToList();
            var displayLogList = new ListView(displayLogEntries, itemHeight: 20, makeItem: () => new Label(),
                bindItem: (item, index) =>
                {
                    (item as Label).text =
                        $"Date: {DateTimeOffset.FromUnixTimeMilliseconds(displayLogEntries[index].displayedOn).ToString("F")}";
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
                if (_selectedOffer != null)
                    _selectedOffer.customPayload =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(evt.newValue);
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
                if (_selectedOffer != null)
                    _selectedOffer.creativeOverride =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(evt.newValue);
            });
            priceField.RegisterValueChangedCallback(evt =>
            {
                if (_selectedOffer != null)
                {
                    _selectedOffer.price = evt.newValue;
                }
            });
        }

        // Used in the settings panel
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

            var placementsListView = new ListView(placements, itemHeight: 20, makeItem: () =>
                {
                    var txt = new TextField();
                    txt.RegisterValueChangedCallback(evt =>
                    {
                        var idx = placements.IndexOf(evt.previousValue);
                        if (idx >= 0)
                        {
                            placements[idx] = evt.newValue;
                        }
                    });
                    return txt;
                },
                bindItem: (item, index) => { (item as TextField).value = placements[index]; })
            {
                selectionType = SelectionType.Single,
                showBorder = true,
                style =
                {
                    flexGrow = 1
                }
            };
            placementsListView.selectionChanged += OnPlacementSelectionChange;

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
                placements.Add(_placementInput.value);
                _placementInput.value = "";
                placementsListView.Rebuild();
            })
            {
                text = "Add"
            };

            // Remove button
            _deletePlacementButton = new Button(() =>
            {
                if (selectedPlacement >= 0 && selectedPlacement < placements.Count)
                {
                    placements.RemoveAt(selectedPlacement);
                    placementsListView.Rebuild();
                    ClearPlacementSelection();
                }
            })
            {
                text = "Remove",
                style = { marginTop = 5 }
            };
            _deletePlacementButton.SetEnabled(false); // Initially disabled

            group.Add(placementsListView);
            group.Add(_deletePlacementButton);

            var addNew = new Foldout { text = "Add new" };
            group.Add(addNew);

            // group.Add(new Label("Add new placement:"));
            addNew.Add(_placementInput);
            addNew.Add(addButton);
        }

        private readonly List<string> templates = new()
        {
            "Offer Display",
            "Offer Interaction",
            "Offer Purchase",
            "User Properties Update",
            "Custom Event"
        };

        private void PopulateEventsPanel()
        {
            PopulateEventsSubmissionPanel();
            PopulateEventsQueuePanel();
        }

        private void PopulateEventsQueuePanel()
        {
            var foldout = new Foldout();
            foldout.text = "Events Queue";

            var logger = _editorAPI.GetEventsLogger();
            var events = logger != null ? logger.EventsQueue : new List<Dictionary<string, object>>();
            var eventsListView = new ListView(events, itemHeight: 20, makeItem: () =>
                    new TextField("Payload", 4096, true, false, '*')
                    {
                        isReadOnly = true,
                        multiline = true,
                        style =
                        {
                            height = 100,
                            flexGrow = 1
                        }
                    },
                bindItem: (item, index) =>
                {
                    // (item as TextElement).text = JsonConvert.SerializeObject(events[index], Formatting.Indented);
                    (item as TextField).value = JsonConvert.SerializeObject(events[index]);
                })
            {
                selectionType = SelectionType.Single,
                showBorder = true,
                style =
                {
                    flexGrow = 1
                }
            };

            rootVisualElement.Add(foldout);
            foldout.Add(eventsListView);

            var flushQueueBtn = new Button(() =>
            {
                _editorAPI.GetEventsLogger().FlushEvents();
                PopulateView();
            });
            flushQueueBtn.text = "Flush Queue";
            rootVisualElement.Add(flushQueueBtn);
        }

        private void PopulateEventsSubmissionPanel()
        {
            var eventTemplates = new DropdownField("Templates")
            {
                index = selectedEventTemplate,
                value = templates[selectedEventTemplate],
                choices = new List<string>(templates)
            };
            eventTemplates.RegisterValueChangedCallback(evt =>
            {
                selectedEventTemplate = templates.IndexOf(evt.newValue);
                eventTemplates.value = templates[selectedEventTemplate];
                PopulateView();
            });

            var eventsJson = new TextField("Event Payload", 4096, true, false, '*');
            eventsJson.RegisterValueChangedCallback(evt => { customEventPayload = evt.newValue; });

            var offerIdField = new TextField("Offer ID");
            offerIdField.value = eventOfferId;
            offerIdField.RegisterValueChangedCallback(evt => { eventOfferId = evt.newValue; });

            var placementOptions = new DropdownField("Placements", placements, 0);
            placementOptions.value = eventPlacementId;
            placementOptions.RegisterValueChangedCallback(evt =>
                eventPlacementId = evt.newValue);

            var amountField = new DoubleField("Amount");
            amountField.value = eventAmount;
            amountField.RegisterValueChangedCallback(evt => { eventAmount = evt.newValue; });

            var currencyField = new TextField("Currency Code");
            currencyField.value = eventCurrency;
            currencyField.RegisterValueChangedCallback(evt => { eventCurrency = evt.newValue; });

            var interactionTypeField = new TextField("Interaction Type");
            interactionTypeField.value = eventInteractionType;
            interactionTypeField.RegisterValueChangedCallback(evt => { eventInteractionType = evt.newValue; });

            var submitButton = new Button
            {
                text = "Submit Event",
                style =
                {
                    marginTop = 15
                }
            };

            rootVisualElement.Add(eventTemplates);

            switch (selectedEventTemplate)
            {
                case 0:
                    rootVisualElement.Add(offerIdField);
                    rootVisualElement.Add(placementOptions);
                    submitButton.RegisterCallback<MouseUpEvent>(evt =>
                    {
                        _editorAPI.LogOfferDisplay(eventOfferId, eventPlacementId);
                        PopulateView();
                    });
                    break;
                case 1:
                    rootVisualElement.Add(offerIdField);
                    rootVisualElement.Add(placementOptions);
                    rootVisualElement.Add(interactionTypeField);
                    submitButton.RegisterCallback<MouseUpEvent>(evt =>
                    {
                        _editorAPI.LogOfferInteraction(eventOfferId, eventPlacementId, eventInteractionType);
                        PopulateView();
                    });
                    break;
                case 2:
                    rootVisualElement.Add(offerIdField);
                    rootVisualElement.Add(placementOptions);
                    rootVisualElement.Add(amountField);
                    rootVisualElement.Add(currencyField);
                    submitButton.RegisterCallback<MouseUpEvent>(evt =>
                    {
                        _editorAPI.LogOfferPurchase(eventOfferId, eventPlacementId, eventAmount, eventCurrency);
                        PopulateView();
                    });
                    break;
                case 3:
                case 4:
                    eventsJson.value = customEventPayload;
                    rootVisualElement.Add(eventsJson);
                    submitButton.RegisterCallback<MouseUpEvent>(evt =>
                    {
                        try
                        {
                            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventsJson.value);
                            if (selectedEventTemplate == 3)
                            {
                                _editorAPI.LogUserAttributes(json);
                            }
                            else
                            {
                                _editorAPI.LogUserEvent(json);
                            }

                            PopulateView();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Error while submitting event: " + e);
                        }
                    });
                    break;
            }

            rootVisualElement.Add(submitButton);
        }

        private void OnPlacementSelectionChange(IEnumerable<object> selection)
        {
            var selectedItems = new List<object>(selection);
            if (selectedItems.Count > 0)
            {
                var selectedValue = selectedItems[0] as string;
                selectedPlacement = placements.IndexOf(selectedValue);
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
            selectedPlacement = -1;
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
            if (_selectedOffer == null) return;

            _offers.Remove(_selectedOffer);
            _offersListView.Rebuild();
            _selectedOffer = null;
            _deleteButton.SetEnabled(false);
            ClearSelectedOfferDetails();
        }
    }
}